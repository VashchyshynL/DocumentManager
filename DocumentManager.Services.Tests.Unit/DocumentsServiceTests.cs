using DocumentManager.Persistence.Models;
using DocumentManager.Persistence.Repositories;
using DocumentManager.Persistence.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentManager.Services.Tests.Unit
{
    [TestClass]
    public class DocumentsServiceTests
    {
        private Document[] _allDocuments; 

        private Mock<IDocumentsRepository> _documentsRepositoryMock;
        private Mock<IContentStorage> _contentStorageMock;
        private DocumentsService _sut;

        [TestInitialize]
        public void Init()
        {
            _allDocuments = new[] {
                new Document { Id = "1", Name = "1.pdf", Position = 1, Location = "storage_location/1.pdf" },
                new Document { Id = "2", Name = "2.pdf", Position = 2, Location = "storage_location/2.pdf" },
                new Document { Id = "3", Name = "3.pdf", Position = 3, Location = "storage_location/3.pdf" },
                new Document { Id = "4", Name = "4.pdf", Position = 4, Location = "storage_location/4.pdf" }
            };

            _documentsRepositoryMock = new Mock<IDocumentsRepository>();
            _contentStorageMock = new Mock<IContentStorage>();

            _sut = new DocumentsService(_documentsRepositoryMock.Object, _contentStorageMock.Object);
        }

        [TestMethod]
        public void GetDocumentsCount_Should_ReturnAllDocumentsCount()
        {
            // Arrange
            var documentsCount = _allDocuments.Count();
            _documentsRepositoryMock
                .Setup(r => r.GetDocumentsCount())
                .Returns(documentsCount);

            // Act
            var actualResult = _sut.GetDocumentsCount();

            // Assert
            Assert.AreEqual(documentsCount, actualResult);
        }

        [TestMethod]
        public async Task GetDocumentsAsync_Should_ReturnAllDocuments()
        {
            // Arrange
            _documentsRepositoryMock
                .Setup(r => r.GetDocumentsAsync())
                .Returns(Task.FromResult(_allDocuments));

            // Act
            var actualResult = await _sut.GetDocumentsAsync();

            // Assert
            Assert.AreEqual(_allDocuments, actualResult);
        }

        [DataTestMethod]
        public async Task GetDocumentByIdAsync_Should_ReturnDocument()
        {
            // Arrange
            var document = _allDocuments.First();
            _documentsRepositoryMock
                .Setup(r => r.GetDocumentByIdAsync(document.Id))
                .Returns(Task.FromResult(document));

            // Act
            var actualResult = await _sut.GetDocumentByIdAsync(document.Id);

            // Assert
            Assert.AreEqual(document, actualResult);
        }

        [TestMethod]
        public async Task SaveDocumentAsync_Should_ReturnDocument_When_Saved()
        {
            // Arrange
            const string fileName = "file.pdf";
            const string location = "storage_location/file.pdf";
            const long fileSize = 1000;

            var documentsCount = _allDocuments.Count();

            _contentStorageMock
                .Setup(s => s.SaveFile(It.IsAny<Stream>(), It.Is<string>(name => name.EndsWith(Path.GetExtension(fileName)))))
                .Returns(Task.FromResult(location));

            _documentsRepositoryMock
                .Setup(r => r.GetDocumentsCount())
                .Returns(_allDocuments.Count());

            _documentsRepositoryMock
                .Setup(r => r.AddDocumentAsync(It.IsAny<Document>()))
                .Returns((Document doc) => Task.FromResult(doc));

            // Act
            var actualResult = await _sut.SaveDocumentAsync(It.IsAny<Stream>(), fileName, fileSize);

            // Assert
            Assert.IsNotNull(actualResult.Id);
            Assert.AreEqual(fileName, actualResult.Name);
            Assert.AreEqual(fileSize, actualResult.FileSize);
            Assert.AreEqual(location, actualResult.Location);
            Assert.AreEqual(documentsCount + 1, actualResult.Position);
        }

        [DataTestMethod]
        [DataRow(1, 3)]
        [DataRow(2, 2)]
        [DataRow(3, 1)]
        [DataRow(4, 0)]
        public async Task DeleteDocumentAsync_Should_DeleteDocumentAndUpdateAffectedDocumentPositions(
            int documentToDeletePosition, int affectedDocumentsCount)
        {
            // Arrange
            var documentToDelete = _allDocuments[documentToDeletePosition - 1];

            _documentsRepositoryMock
                .Setup(r => r.GetDocumentsAsync())
                .Returns(Task.FromResult(_allDocuments));

            // Act
            await _sut.DeleteDocumentAsync(documentToDelete);

            // Assert
            _documentsRepositoryMock
                .Verify(r => r.DeleteDocumentAsync(documentToDelete.Id, It.Is<Document[]>(
                    documents => documents.Length == affectedDocumentsCount 
                         && (!documents.Any() || documents.All(x => x.Position == int.Parse(x.Id) - 1)))));

            _contentStorageMock
                .Verify(s => s.DeleteFile(It.Is<string>(name => name == Path.GetFileName(documentToDelete.Location))));
        }

        [TestMethod]
        public async Task InsertDocumentToPositionAsync_Should_ReturnFromMethod_When_PositionAlreadySet()
        {
            // Arrange
            var documentToInsert = _allDocuments.First();

            // Act
            await _sut.InsertDocumentToPositionAsync(documentToInsert, documentToInsert.Position);

            // Assert
            _documentsRepositoryMock.Verify(r => r.UpdateDocumentsAsync(It.IsAny<List<Document>>()), Times.Never);
        }

        [DataTestMethod]
        [DataRow(1, 2, 2, -1)]
        [DataRow(1, 3, 3, -1)]
        [DataRow(1, 4, 4, -1)]
        [DataRow(2, 1, 2, 1)]
        [DataRow(2, 3, 2, -1)]
        [DataRow(2, 4, 3, -1)]
        [DataRow(3, 1, 3, 1)]
        [DataRow(3, 2, 2, 1)]
        [DataRow(3, 4, 2, -1)]
        [DataRow(4, 1, 4, 1)]
        [DataRow(4, 2, 3, 1)]
        [DataRow(4, 3, 2, 1)]
        public async Task InsertDocumentToPositionAsync_Should_InsertDocumentAndShiftAffectedDocumentPositions(
            int oldPosition, int newPosition, int documentsToUpdate, int shiftValue)
        {
            // Arrange
            var documentToInsert = _allDocuments[oldPosition - 1];

            _documentsRepositoryMock
                .Setup(r => r.GetDocumentsAsync())
                .Returns(Task.FromResult(_allDocuments));

            // Act
            await _sut.InsertDocumentToPositionAsync(documentToInsert, newPosition);

            // Assert
            _documentsRepositoryMock
                .Verify(r => r.UpdateDocumentsAsync(It.Is<List<Document>>(documents => 
                    documents.Count == documentsToUpdate
                    && documents.All(x => x.Id == documentToInsert.Id || x.Position == int.Parse(x.Id) + shiftValue))));
        }
    }
}
