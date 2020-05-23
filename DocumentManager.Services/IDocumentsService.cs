using DocumentManager.Persistence.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DocumentManager.Services
{
    public interface IDocumentsService
    {
        /// <summary>
        /// Get documents count
        /// </summary>
        int GetDocumentsCount();

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
        /// Save document (asynchronously)
        /// </summary>
        /// <param name="fileStream">File stream</param>
        /// <param name="fileName">File name</param>
        /// <param name="fileSize">File size</param>
        Task<Document> SaveDocumentAsync(Stream fileStream, string fileName, long fileSize);

        /// <summary>
        /// Delete document (asynchronously)
        /// </summary>
        /// <param name="document">Document for deletion</param>
        Task DeleteDocumentAsync(Document document);

        /// <summary>
        /// Insert document to specific position (asynchronously)
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="position">New document position</param>
        Task InsertDocumentToPositionAsync(Document document, int position);
    }
}
