using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentManager.Api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DocumentManager.Api.Services
{
    public class CosmosDbService : IDbService
    {
        private Container _container;
        private readonly ILogger<CosmosDbService> _logger;

        public CosmosDbService(Container container, ILogger<CosmosDbService> logger)
        {
            _container = container;
            _logger = logger;
        }

        public async Task<IEnumerable<Document>> GetDocumentsAsync()
        {
            try
            {
                _logger.LogInformation($"Retrieving all documents from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' started");


                var documents = await Task.Factory.StartNew<IEnumerable<Document>>(
                    () => _container.GetItemLinqQueryable<Document>(true));

                _logger.LogInformation($"Retrieving all documents from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' finished");

                return documents;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of retrieving documents from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}'");
                throw ex;
            }
        }

        public async Task<Document> GetDocumentByIdAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Retrieving document by Id: '{id}' from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' started");

                var response = await _container.ReadItemAsync<Document>(id, new PartitionKey(id));

                _logger.LogInformation($"Retrieving document by Id: '{id}' from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' finished");

                return response?.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(ex, $"Document with Id: '{id}' not found in CosmosDb: '{_container.Database.Id}' container: '{_container.Id}'");
                return null;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of retrieving document with Id: '{id}' from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}'");
                throw ex;
            }
        }

        public async Task AddDocumentAsync(Document document)
        {
            try
            {
                _logger.LogInformation($"Adding Document '{document.Name}' to CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' started");

                await _container.CreateItemAsync(document, new PartitionKey(document.Id));

                _logger.LogInformation($"Adding Document '{document.Name}' to CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' started");
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of adding document '{document.Name}' to CosmosDb: '{_container.Database.Id}' container: '{_container.Id}'");
                throw ex;
            }
        }

        public async Task DeleteDocumentAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Deleting document with Id: '{id}' from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' started");

                await _container.DeleteItemAsync<Document>(id, new PartitionKey(id));

                _logger.LogInformation($"Deleting document with Id: '{id}' from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}' finished");

            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex, $"Error of deleting document with Id: '{id}' from CosmosDb: '{_container.Database.Id}' container: '{_container.Id}'");
                throw ex;
            }
        }
    }
}
