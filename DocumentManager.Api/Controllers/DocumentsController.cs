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
            {
                _logger.LogWarning($"Document with Id: '{id}' not found");
                return NotFound();
            }
            
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
            if(file == null)
                return BadRequest();

            var id = Guid.NewGuid().ToString();
            string fileLocation;
            using (var stream = file.OpenReadStream())
            {
                fileLocation = await _contentService.SaveFile(stream, id + Path.GetExtension(file.FileName));
            }

            var document = new Document {
                Id = id,
                Name = file.FileName,
                FileSize = file.Length,
                Location = fileLocation
            };

            await _dbService.AddDocumentAsync(document);

            return CreatedAtAction("Get", new { id = document.Id }, document);
        }

        /// <summary>
        /// Action method for deleting particular document by it's Id
        /// </summary>
        /// <param name="id">Id of the document</param>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var document = await _dbService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                _logger.LogWarning($"Document with Id: '{id}' not found");
                return NotFound();
            }

            await _dbService.DeleteDocumentAsync(id);
            _logger.LogInformation($"Document with Id: '{id}' deleted from DB");

            await _contentService.DeleteFile(Path.GetFileName(document.Location));

            return NoContent();
        }
    }
}
