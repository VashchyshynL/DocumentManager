using DocumentManager.Persistence.Configuration;
using DocumentManager.Persistence.Exceptions;
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

            try
            {
                var azureBlobSettings = configuration.GetSection("AzureBlobStorage").Get<StorageSettings>();

                var credentials = new StorageCredentials(azureBlobSettings.Name, azureBlobSettings.Key);
                var storageAccount = new CloudStorageAccount(credentials, true);
                var blobClient = storageAccount.CreateCloudBlobClient();

                _container = blobClient.GetContainerReference(azureBlobSettings.Container);
            }
            catch (Exception ex)
            {
                throw CreatePersistenceException("Error of connecting to Azure Blob Storage container", ex);
            }
        }

        public async Task<string> SaveFile(Stream stream, string fileName)
        {
            try
            {
                using (_logger.BeginScope($"Saving '{fileName}' file to Azure Blob Storage container: '{_container.Name}'"))
                {
                    var blockBlob = _container.GetBlockBlobReference(fileName);

                    await blockBlob.UploadFromStreamAsync(stream);

                    var location = blockBlob.Uri.ToString();
                    _logger.LogInformation($"File '{fileName}' successfully stored to Azure Blob Storage location: '{location}'");

                    return location;
                }
            }
            catch (StorageException ex)
            {
                throw CreatePersistenceException($"Error during saving '{fileName}' file to Azure Blob Storage container: '{_container.Name}'", ex);
            }
        }

        public async Task DeleteFile(string fileName)
        {
            try
            {
                using (_logger.BeginScope($"Deleting '{fileName}' file from Azure Blob Storage container: '{_container.Name}'"))
                {
                    var blockBlob = _container.GetBlockBlobReference(fileName);
                    await blockBlob.DeleteIfExistsAsync();
                }
            }
            catch (StorageException ex)
            {
                throw CreatePersistenceException($"Error during deleting '{fileName}' file from Azure Blob Storage container: '{_container.Name}'", ex);
            }
        }

        private PersistenceException CreatePersistenceException(string errorMessage, Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            return new PersistenceException(errorMessage, ex);
        }
    }
}
