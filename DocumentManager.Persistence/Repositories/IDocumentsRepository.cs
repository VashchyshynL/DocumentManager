using DocumentManager.Persistence.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentManager.Persistence.Repositories
{
    /// <summary>
    /// Interface for interaction with documents repository
    /// </summary>
    public interface IDocumentsRepository
    {
        /// <summary>
        /// Get documents count
        /// </summary>
        int GetDocumentsCount();

        /// <summary>
        /// Get all documents (asynchronously)
        /// </summary>
        /// <returns>Documents collection</returns>
        Task<Document[]> GetDocumentsAsync();

        /// <summary>
        /// Obtain document by its Id (asynchronously)
        /// </summary>
        /// <param name="id">Id of the document</param>
        /// <returns>Document if exist, otherwise null</returns>
        Task<Document> GetDocumentByIdAsync(string id);

        /// <summary>
        /// Add document to repository
        /// </summary>
        /// <param name="document">Document object</param>
        /// <returns>Newly added document</returns>
        Task<Document> AddDocumentAsync(Document document);

        /// <summary>
        /// Delete document from repository
        /// </summary>
        /// <param name="id">Id of the document for deletion</param>
        /// <param name="documentsToUpdate">Collection of documents affected by deletion</param>
        Task DeleteDocumentAsync(string id, IReadOnlyCollection<Document> documentsToUpdate);

        /// <summary>
        /// Update documents (asynchronously)
        /// </summary>
        /// <param name="documentsToUpdate">Collection of documents for update</param>
        Task UpdateDocumentsAsync(IReadOnlyCollection<Document> documentsToUpdate);
    }
}
