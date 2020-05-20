using DocumentManager.Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManager.Api.Services
{
    /// <summary>
    /// Service for interaction with DataBase
    /// </summary>
    public interface IDbService
    {
        /// <summary>
        /// Get all documents (asynchronously)
        /// </summary>
        /// <returns>Enumeration of documents</returns>
        Task<IEnumerable<Document>> GetDocumentsAsync();

        /// <summary>
        /// Obtain document by its Id (asynchronously)
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>Document if exist, otherwise null</returns>
        Task<Document> GetDocumentByIdAsync(string id);

        /// <summary>
        /// Add document to DataDase
        /// </summary>
        /// <param name="document">Document object</param>
        Task AddDocumentAsync(Document document);

        /// <summary>
        /// Delete document from DataBase
        /// </summary>
        /// <param name="id">Id of the document</param>
        Task DeleteDocumentAsync(string id);
    }
}
