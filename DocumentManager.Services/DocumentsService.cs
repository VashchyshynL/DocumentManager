using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentManager.Persistence.Models;
using DocumentManager.Persistence.Repositories;
using DocumentManager.Persistence.Storage;

namespace DocumentManager.Services
{
    public class DocumentsService : IDocumentsService
    {
        private readonly IDocumentsRepository _documentsRepository;
        private readonly IContentStorage _contentStorage;

        public DocumentsService(IDocumentsRepository documentsRepository, IContentStorage contentStorage)
        {
            _documentsRepository = documentsRepository;
            _contentStorage = contentStorage;
        }

        public int GetDocumentsCount()
        {
            return _documentsRepository.GetDocumentsCount();
        }

        public Task<Document[]> GetDocumentsAsync()
        {
            return _documentsRepository.GetDocumentsAsync();
        }

        public Task<Document> GetDocumentByIdAsync(string id)
        {
            return _documentsRepository.GetDocumentByIdAsync(id);
        }

        public async Task<Document> SaveDocumentAsync(Stream fileStream, string fileName, long fileSize)
        {
            var id = Guid.NewGuid().ToString();
            var fileLocation = await _contentStorage.SaveFile(fileStream, id + Path.GetExtension(fileName));

            var documentsCount = _documentsRepository.GetDocumentsCount();

            var document = new Document
            {
                Id = id,
                Name = fileName,
                FileSize = fileSize,
                Location = fileLocation,
                Position = documentsCount + 1
            };

            return await _documentsRepository.AddDocumentAsync(document);
        }

        public async Task DeleteDocumentAsync(Document document)
        {
            var allDocuments = await _documentsRepository.GetDocumentsAsync();
            var documentsToUpdate = GetInsertionAffectedDocuments(allDocuments, document.Position, allDocuments.Length).ToArray();

            await _documentsRepository.DeleteDocumentAsync(document.Id, documentsToUpdate);
            await _contentStorage.DeleteFile(Path.GetFileName(document.Location));
        }

        public async Task InsertDocumentToPositionAsync(Document document, int position)
        {
            if (document.Position == position)
                return;

            var allDocuments = await _documentsRepository.GetDocumentsAsync();
            var documentsToUpdate = GetInsertionAffectedDocuments(allDocuments, document.Position, position).ToList();

            document.Position = position;
            documentsToUpdate.Add(document);

            await _documentsRepository.UpdateDocumentsAsync(documentsToUpdate);
        }


        /// <summary>
        /// Get documents affected by insertion of existing document into particular position
        /// </summary>
        /// <param name="documents">All documents</param>
        /// <param name="oldPosition">Old document position</param>
        /// <param name="newPosition">New document position</param>
        /// <returns>Enumeration of affected documents</returns>
        private IEnumerable<Document> GetInsertionAffectedDocuments(Document[] documents, int oldPosition, int newPosition)
        {
            // shifting documents to the left
            for (int i = oldPosition; i < newPosition; i++)
            {
                var document = documents[i];
                document.Position--;
                yield return document;
            }

            // shifting documents to the right
            for (int i = newPosition; i < oldPosition; i++)
            {
                var document = documents[i - 1];
                document.Position++;
                yield return document;
            }
        }
    }
}
