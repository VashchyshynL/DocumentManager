using DocumentManager.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManager.Api.Services
{
    public interface IDbService
    {
        Task<IEnumerable<Document>> GetDocumentsAsync();
        Task<Document> GetDocumentByIdAsync(string id);
        Task AddDocumentAsync(Document document);
        Task DeleteDocumentAsync(string id);
    }
}
