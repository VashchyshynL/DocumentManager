using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManager.Api.Services
{
    public class AzureBlobContentService : IContentService
    {
        private readonly CloudBlobContainer _container;

        public AzureBlobContentService(CloudBlobContainer container)
        {
            _container = container;
        }

        public async Task<string> SaveFile(Stream stream, string fileName)
        {
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference(fileName);

            await blockBlob.UploadFromStreamAsync(stream);

            return blockBlob?.Uri?.ToString();
        }

        public async Task DeleteFile(string filePath)
        {
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference(filePath);

            await blockBlob.DeleteIfExistsAsync();
        }
    }
}
