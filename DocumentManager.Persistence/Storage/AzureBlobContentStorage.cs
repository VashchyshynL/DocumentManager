using DocumentManager.Persistence.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManager.Persistence.Storage
{
    public class AzureBlobContentStorage : IContentStorage
    {
        private readonly CloudBlobContainer _container;
        private readonly ILogger<AzureBlobContentStorage> _logger;

        public AzureBlobContentStorage(IConfiguration configuration, ILogger<AzureBlobContentStorage> logger)
        {
            _logger = logger;

            var azureBlobSettings = configuration.GetSection("AzureBlobStorage").Get<StorageSettings>();

            var credentials = new StorageCredentials(azureBlobSettings.Name, azureBlobSettings.Key);
            var storageAccount = new CloudStorageAccount(credentials, true);
            var blobClient = storageAccount.CreateCloudBlobClient();

            _container = blobClient.GetContainerReference(azureBlobSettings.Container);
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
                _logger.LogError(ex, $"Error during saving file '{fileName}' to Azure Blob Storage container: '{_container.Name}'");
                throw ex;
            }
        }

        public async Task DeleteFile(string filePath)
        {
            try
            {
                using (_logger.BeginScope($"Deleting file '{filePath}' from Azure Blob Storage container: '{_container.Name}'"))
                {
                    var blockBlob = _container.GetBlockBlobReference(filePath);
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
