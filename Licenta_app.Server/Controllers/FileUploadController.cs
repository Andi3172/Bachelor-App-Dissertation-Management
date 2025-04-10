using Licenta_app.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Licenta_app.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public FileUploadController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // get all files
        // GET: api/fileupload
        [Authorize(Roles = "Professor, Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileUpload>>> GetAllFiles()
        {
            return await _context.Files.Include(f => f.RegistrationRequest).ToListAsync();
        }

        // get file by id
        // GET: api/fileupload/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FileUpload>> GetFileById(int id)
        {
            var file = await _context.Files
                .Include(f => f.RegistrationRequest)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            return file;
        }

        // upload file (handle file upload)
        // POST: api/fileupload

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, int requestId, string uploadedBy)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";

            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty or none was uploaded");
            }

            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request == null)
            {
                return BadRequest("Invalid request ID");
            }

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (file.Length > 50 * 1024 * 1024)
            {
                return BadRequest("File size exceeds 50MB");
            }

            var newFile = new FileUpload
            {
                RequestId = requestId,
                UploadedBy = uploadedBy,
                FileName = file.FileName,
                FilePath = filePath,
                FileType = file.ContentType,
                UploadedDate = DateTime.UtcNow,
                RegistrationRequest = request
            };

            _context.Files.Add(newFile);
            await _context.SaveChangesAsync();

            return Ok(new { message = "File uploaded succsessfully", fileId = newFile.Id });
        }

        // download file
        // GET: api/fileupload/download/5
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(int id)
        {   
            var file = await _context.Files.FindAsync(id);
            if(file == null)
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(file.FilePath))
            {
                return NotFound("File not found on server.");
            }

            var normalizedFilePath = Path.GetFullPath(file.FilePath);
            var normalizedWebRootPath = Path.GetFullPath(_environment.WebRootPath);

            if (!normalizedFilePath.StartsWith(normalizedWebRootPath, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Access denied");
            }

            var contentType = !string.IsNullOrEmpty(file.FileType) ? file.FileType : "application/octet-stream";

            var memory = new MemoryStream();
            using (var stream = new FileStream(file.FilePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            

            return File(memory, contentType, file.FileName);
        }

        //idk if this should be only for admins or for anyone
        // delete file by id
        // DELETE: api/fileupload/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }
            if (System.IO.File.Exists(file.FilePath))
            {
                System.IO.File.Delete(file.FilePath);
            }
            _context.Files.Remove(file);
            await _context.SaveChangesAsync();
            return Ok( new {message = "File deleted successfully", fileId = file.Id});
        }

        

    }
}
