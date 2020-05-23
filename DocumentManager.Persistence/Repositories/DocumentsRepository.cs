﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentManager.Persistence.Models;
using DocumentManager.Persistence.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocumentManager.Persistence.Repositories
{
    public class DocumentsRepository : IDocumentsRepository
    {
        private readonly DbSettings _dbSettings;
        private readonly ILogger<DocumentsRepository> _logger;

        private Container _container;

        public DocumentsRepository(IConfiguration configuration, ILogger<DocumentsRepository> logger)
        {
            _dbSettings = configuration.GetSection("CosmosDb").Get<DbSettings>();
            _logger = logger;

            var clientBuilder = new CosmosClientBuilder(_dbSettings.Endpoint, _dbSettings.Key);
            var client = clientBuilder.WithConnectionModeDirect().Build();
            _container = client.GetContainer(_dbSettings.Database, _dbSettings.Container);
        }

        public int GetDocumentsCount()
        {
            try
            {
                return _container.GetItemLinqQueryable<Document>(true).Count();
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of retrieving documents count from CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'");
                throw ex;
            }
        }

        public async Task<Document[]> GetDocumentsAsync()
        {
            try
            {
                return await Task.Factory.StartNew(
                    () => _container.GetItemLinqQueryable<Document>(true).OrderBy(d => d.Position).ToArray());
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of retrieving documents from CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'");
                throw ex;
            }
        }

        public async Task<Document> GetDocumentByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Document>(id, new PartitionKey(_dbSettings.PartitionKey));

                return response?.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, $"Document '{id}' not found in CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'");
                return null;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of retrieving document '{id}' from CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'");
                throw ex;
            }
        }

        public async Task<Document> AddDocumentAsync(Document document)
        {
            try
            {
                using (_logger.BeginScope($"Adding Document '{document.Name}' to CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'"))
                {
                    document.Partition = _dbSettings.PartitionKey;
                    var response = await _container.CreateItemAsync(document, new PartitionKey(_dbSettings.PartitionKey));

                    return response.Resource;
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of adding document '{document.Name}' to CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'");
                throw ex;
            }
        }

        public async Task DeleteDocumentAsync(string id, IReadOnlyCollection<Document> documentsToUpdate)
        {
            using (_logger.BeginScope($"Deleting document '{id}' from CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'"))
            {
                var batch = _container.CreateTransactionalBatch(new PartitionKey(_dbSettings.PartitionKey));

                batch.DeleteItem(id);

                foreach (var document in documentsToUpdate)
                    batch.UpsertItem(document);

                using (var batchResponse = await batch.ExecuteAsync())
                {
                    if (batchResponse.IsSuccessStatusCode)
                        _logger.LogInformation($"Document '{id}' was deleted from CosmosDb");
                    else
                        _logger.LogError($"Transaction of deleting document '{id}' failed", batchResponse.ErrorMessage);
                }
            }
        }

        public async Task UpdateDocumentsAsync(IReadOnlyCollection<Document> documentsToUpdate)
        {
            if (!documentsToUpdate.Any())
            {
                _logger.LogWarning("Trying to update empty collection");
                return;
            }

            using (_logger.BeginScope($"Updating documents '{string.Join(", ", documentsToUpdate.Select(d => d.Id))}' in CosmosDb: '{_dbSettings.Database}' container: '{_container.Id}'"))
            {

                var batch = _container.CreateTransactionalBatch(new PartitionKey(_dbSettings.PartitionKey));

                foreach (var document in documentsToUpdate)
                    batch.UpsertItem(document);

                using (var batchResponse = await batch.ExecuteAsync())
                {
                    if (batchResponse.IsSuccessStatusCode)
                        _logger.LogError($"Updating transaction failed", batchResponse.ErrorMessage);
                }
            }
        }
    }
}
