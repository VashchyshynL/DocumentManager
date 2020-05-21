using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManager.Api.Models;
using DocumentManager.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DocumentManager.Api.Dtos;
using DocumentManager.Api.Validators;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DocumentManager.Api.Controllers
{
    /// <summary>
    /// Controller for handling documents management
    /// </summary>
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private readonly IContentService _contentService;

        private IMapper _mapper;
        private ILogger<DocumentsController> _logger;

        public DocumentsController(IDbService dbService, IContentService contentService, IMapper mapper, ILogger<DocumentsController> logger)
        {
            _dbService = dbService;
            _contentService = contentService;

            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Action method for retrieving all pdf documents which are in the system
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll()
        {
            var documents = await _dbService.GetDocumentsAsync();

            return Ok(_mapper.Map<IEnumerable<DocumentDto>>(documents));
        }

        /// <summary>
        /// Action method for retrieving particular document by it's Id
        /// </summary>
        /// <param name="id">Id of the document</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentDto>> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var document = await _dbService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            return Ok(_mapper.Map<DocumentDto>(document));
        }

        /// <summary>
        /// Action method for uploading document into system 
        /// </summary>
        /// <param name="file">Uploading file</param>
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload([PdfFormFile(5)] IFormFile file)
        {
            if (file == null)
                return BadRequest();

            var id = Guid.NewGuid().ToString();
            string fileLocation;
            using (var stream = file.OpenReadStream())
            {
                fileLocation = await _contentService.SaveFile(stream, id + Path.GetExtension(file.FileName));
            }

            var documentsCount = _dbService.GetDocumentsCount();

            var document = new Document
            {
                Id = id,
                Name = file.FileName,
                FileSize = file.Length,
                Location = fileLocation,
                Position = documentsCount + 1
            };

            await _dbService.AddDocumentAsync(document);

            return CreatedAtAction("Get", new { id = document.Id }, _mapper.Map<DocumentDto>(document));
        }

        /// <summary>
        /// Action method for deleting particular document by it's Id
        /// </summary>
        /// <param name="id">Id of the document</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var document = await _dbService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            var documents = (await _dbService.GetDocumentsAsync()).ToArray();
            var documentsToUpdate = GetInsertionAffectedDocuments(documents, document.Position, documents.Length).ToArray();

            await _dbService.DeleteDocumentAsync(id, documentsToUpdate);
            await _contentService.DeleteFile(Path.GetFileName(document.Location));

            return NoContent();
        }

        /// <summary>
        /// Action method for updating document position
        /// </summary>
        /// <param name="id">Id of the document<</param>
        /// <param name="position">Position to insert</param>
        /// <returns></returns>
        [HttpPatch("{id}", Name = "PartiallyUpdateBookForAuthor")]
        public async Task<IActionResult> InsertToPosition(string id, [FromBody, Range(1, int.MaxValue)] int position)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var document = await _dbService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            var oldPosition = document.Position;
            if (oldPosition == position)
                return Ok();

            var documentsCount = _dbService.GetDocumentsCount();
            if (position > documentsCount)
            {
                _logger.LogWarning($"Trying to insert document with Id: '{id}' into position: '{position}' (from '{documentsCount}' awailable)");
                return BadRequest($"Position '{position}' is out of the range of existing documents");
            }

            var documents = (await _dbService.GetDocumentsAsync()).ToArray();
            var documentsToUpdate = new List<Document>();

            document.Position = position;
            documentsToUpdate.Add(document);

            var affectedDocuments = GetInsertionAffectedDocuments(documents, oldPosition, position);
            documentsToUpdate.AddRange(affectedDocuments);

            await _dbService.UpdateDocumentsAsync(documentsToUpdate);

            return NoContent();
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
