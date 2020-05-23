using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DocumentManager.Api.Dtos;
using DocumentManager.Api.Validators;
using System.ComponentModel.DataAnnotations;
using DocumentManager.Services;

namespace DocumentManager.Api.Controllers
{
    /// <summary>
    /// Controller for handling documents management
    /// </summary>
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentsService _documentsService;
        private IMapper _mapper;
        private ILogger<DocumentsController> _logger;

        public DocumentsController(IDocumentsService documentsService, IMapper mapper, ILogger<DocumentsController> logger)
        {
            _documentsService = documentsService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Action method for retrieving all pdf documents which are in the system
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DocumentDto>>> GetAll()
        {
            var documents = await _documentsService.GetDocumentsAsync();

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

            var document = await _documentsService.GetDocumentByIdAsync(id);
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

            using (var fileStream = file.OpenReadStream())
            {
                var document = await _documentsService.SaveDocumentAsync(fileStream, file.FileName, file.Length);

                return CreatedAtAction("Get", new { id = document.Id }, _mapper.Map<DocumentDto>(document));
            }
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

            var document = await _documentsService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            await _documentsService.DeleteDocumentAsync(document);

            return NoContent();
        }

        /// <summary>
        /// Action method for updating document position
        /// </summary>
        /// <param name="id">Id of the document<</param>
        /// <param name="position">Position to insert</param>
        [HttpPatch("{id}")]
        public async Task<IActionResult> InsertToPosition(string id, [FromBody, Range(1, int.MaxValue)] int position)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var document = await _documentsService.GetDocumentByIdAsync(id);
            if (document == null)
                return NotFound();

            if (document.Position == position)
                return NoContent();

            var documentsCount = _documentsService.GetDocumentsCount();
            if (position > documentsCount)
            {
                _logger.LogWarning($"Trying to insert document with Id: '{id}' into position: '{position}' (from '{documentsCount}' awailable)");
                return BadRequest($"Position '{position}' is out of the range of existing documents");
            }

            await _documentsService.InsertDocumentToPositionAsync(document, position);

            return NoContent();
        }
    }
}
