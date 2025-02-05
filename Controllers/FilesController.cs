using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Net.Mime;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;
        private readonly CitiesDataStore _dataStore;
        public FilesController(
            FileExtensionContentTypeProvider fileExtensionContentTypeProvider, CitiesDataStore dataStore)
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new System.ArgumentNullException(
                    nameof(fileExtensionContentTypeProvider));
            _dataStore = dataStore;
        }
        [HttpGet("{fileid}")]
        public ActionResult GetFile( string fileid)
        {
            var pathtofile = "Muhavare.pdf";
            if (!System.IO.File.Exists(pathtofile))
            { 
                return NotFound();
            }
            if( !_fileExtensionContentTypeProvider.TryGetContentType(pathtofile, out string contenttype))
            {
                contenttype = "application/octet-stream";
            }
            var bytes = System.IO.File.ReadAllBytes(pathtofile);
            return File(bytes,contenttype,Path.GetFileName(pathtofile));
        }

        [HttpPost]
        public async Task<ActionResult> CreateFile(IFormFile file)
        {
            // Validate the input. Put a limit on filesize to avoid large uploads attacks. 
            // Only accept .pdf files (check content-type)
            if (file.Length == 0 || file.Length > 20971520 || file.ContentType != "application/pdf")
            {
                return BadRequest("No file or an invalid one has been inputted.");
            }

            // Create the file path.  Avoid using file.FileName, as an attacker can provide a
            // malicious one, including full paths or relative paths.  
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                $"uploaded_file_{Guid.NewGuid()}.pdf");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok("Your file has been uploaded successfully.");
        }
    }
}
