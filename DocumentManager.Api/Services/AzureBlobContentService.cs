using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManager.Api.Services
{
    public class AzureBlobContentService : IContentService
    {
        private readonly CloudBlobContainer _container;
        private readonly ILogger<AzureBlobContentService> _logger;

        public AzureBlobContentService(CloudBlobContainer container, ILogger<AzureBlobContentService> logger)
        {
            _container = container;
            _logger = logger;
        }

        public async Task<string> SaveFile(Stream stream, string fileName)
        {
            try
            {
                using (_logger.BeginScope($"Saving file '{fileName}' to Azure Blob Storage container: '{_container.Name}'"))
                {
                    var blockBlob = _container.GetBlockBlobReference(fileName);

                    await blockBlob.UploadFromStreamAsync(stream);

                    var location = blockBlob.Uri.ToString();
                    _logger.LogInformation($"File '{fileName}' successfully stored to Azure Blob Storage location: '{location}'");

                    return location;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error during saving file '{fileName}' to Azure Blob Storage container: '{_container.Name}'");
                throw ex;
            }
        }

        public async Task DeleteFile(string filePath)
        {
            try
            {
                using (_logger.BeginScope($"Deleting file '{filePath}' from Azure Blob Storage container: '{_container.Name}'"))
                {
                    CloudBlockBlob blockBlob = _container.GetBlockBlobReference(filePath);
                    await blockBlob.DeleteIfExistsAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during deleting file '{filePath}' from Azure Blob Storage container: '{_container.Name}'");
                throw ex;
            }
        }
    }
}
