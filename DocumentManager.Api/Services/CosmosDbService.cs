using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentManager.Api.Models;
using Microsoft.Azure.Cosmos;

namespace DocumentManager.Api.Services
{
    public class CosmosDbService : IDbService
    {
        private Container _container;

        public CosmosDbService(Container container)
        {
            _container = container;
        }

        public async Task<IEnumerable<Document>> GetDocumentsAsync()
        {
            return await Task.Factory.StartNew<IEnumerable<Document>>(
                () => _container.GetItemLinqQueryable<Document>(true));
        }

        public async Task<Document> GetDocumentByIdAsync(string id)
        {
            try
            {
                var response = await _container.ReadItemAsync<Document>(id, new PartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // todo: Add logging
                return null;
            }
        }

        public async Task AddDocumentAsync(Document document)
        {
            await _container.CreateItemAsync<Document>(document, new PartitionKey(document.Id));
        }

        public async Task DeleteDocumentAsync(string id)
        {
            await _container.DeleteItemAsync<Document>(id, new PartitionKey(id));
        }
    }
}
