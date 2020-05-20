using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManager.Api.Helpers;
using DocumentManager.Api.Models;
using DocumentManager.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DocumentManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDbService _dbService;
        private readonly IContentService _contentService;
        private readonly IFileValidator _fileValidator;

        private ILogger<DocumentsController> _logger;

        public DocumentsController(IDbService dbService, IContentService contentService, IFileValidator fileValidator, ILogger<DocumentsController> logger)
        {
            _dbService = dbService;
            _contentService = contentService;
            _fileValidator = fileValidator;

            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetAll()
        {
            var documents = await _dbService.GetDocumentsAsync();
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var document = await _dbService.GetDocumentByIdAsync(id);
            if (document == null)
            {
                _logger.LogWarning($"Document with Id: '{id}' not found");
                return NotFound();
            }
            
            return Ok(document);
        }

        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if(file == null)
                return BadRequest();

            if (!_fileValidator.IsValidExtension(file.FileName))
                ModelState.AddModelError(nameof(file), $"File should have '{_fileValidator.Extension}' extension");

            if (_fileValidator.IsExceedsMaxSize(file.Length))
                ModelState.AddModelError(nameof(file), $"File size should not be greater than '{_fileValidator.MaxFileSizeInBytes}' bytes");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = Guid.NewGuid().ToString();
            string fileLocation;
            using (var stream = file.OpenReadStream())
            {
                fileLocation = await _contentService.SaveFile(stream, id + Path.GetExtension(file.FileName));
                _logger.LogInformation($"File '{file.FileName}' successfully saved into storage location: '{fileLocation}'");
            }

            var document = new Document {
                Id = id,
                Name = file.FileName,
                FileSize = file.Length,
                Location = fileLocation
            };

            await _dbService.AddDocumentAsync(document);
            _logger.LogInformation($"Document with Id: '{id}' and file name: '{file.FileName}' successfully stored");

            return CreatedAtAction("Get", new { id = document.Id }, document);
        }

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
            _logger.LogInformation($"Document with Id: '{id}' deleted from storage location: '{document.Location}'");

            return NoContent();
        }
    }
}
