using DocumentManager.Api.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace DocumentManager.Api.Tests.Unit
{
    [TestClass]
    public class PdfFormFileAttributeTests
    {
        private const long MaxFileSizeInBytes = 5242880;

        private Mock<IFormFile> _formFileMock;
        private PdfFormFileAttribute _sut;

        [TestInitialize]
        public void Init()
        {
            _formFileMock = new Mock<IFormFile>();
            _sut = new PdfFormFileAttribute(MaxFileSizeInBytes / 1024 / 1024);
        }

        [TestMethod]
        public void IsValid_Should_SkipValidation_When_AttributeAppliedToNotFormFileParameter()
        {
            // Arrange
            var value = "wrong data type value";

            // Act
            var actualResult = _sut.GetValidationResult(value, new ValidationContext(value));

            // Assert
            Assert.AreEqual(ValidationResult.Success, actualResult);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("file.txt")]
        [DataRow("file.log")]
        public void IsValid_Should_ReturnErrorMessage_When_FileNameInvalid(string fileName)
        {
            // Arrange
            _formFileMock.Setup(x => x.FileName).Returns(fileName);

            // Act
            var actualResult = _sut.GetValidationResult(_formFileMock.Object, new ValidationContext(_formFileMock.Object));

            // Assert
            StringAssert.Contains(actualResult.ErrorMessage, ".pdf");
        }

        [TestMethod]
        public void IsValid_Should_ReturnErrorMessage_When_FileSizeIsGreaterThanAllowed()
        {
            // Arrange
            _formFileMock.Setup(x => x.FileName).Returns("file.pdf");
            _formFileMock.Setup(x => x.Length).Returns(MaxFileSizeInBytes + 1);

            // Act
            var actualResult = _sut.GetValidationResult(_formFileMock.Object, new ValidationContext(_formFileMock.Object));

            // Assert
            StringAssert.Contains(actualResult.ErrorMessage, $"{MaxFileSizeInBytes}");
        }

        [TestMethod]
        public void IsValid_Should_ReturnSuccess_When_FileNameAndSizeAreValid()
        {
            // Arrange
            _formFileMock.Setup(x => x.FileName).Returns("file.pdf");
            _formFileMock.Setup(x => x.Length).Returns(MaxFileSizeInBytes);

            // Act
            var actualResult = _sut.GetValidationResult(_formFileMock.Object, new ValidationContext(_formFileMock.Object));

            // Assert
            Assert.AreEqual(ValidationResult.Success, actualResult);
        }
    }
}
