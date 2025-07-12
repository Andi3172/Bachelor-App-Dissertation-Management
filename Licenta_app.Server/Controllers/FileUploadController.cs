using Licenta_app.Server.Data;
using Licenta_app.Server.Models;
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
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto dto)
        {
            var file = dto.File;
            var requestId = dto.RequestId;
            var uploadedBy = dto.UploadedBy;
            var fileType = dto.FileType;

            if (file == null || file.Length == 0)
            {
                return BadRequest("File is empty or none was uploaded");
            }

            var allowedTypes = new[] { "academic-default", "academic-signed", "progress" };
            if (!allowedTypes.Contains(fileType))
            {
                return BadRequest("Invalid file type. Allowed types are: " + string.Join(", ", allowedTypes));
            }

            var request = await _context.RegistrationRequests
                .Include(r => r.Student).ThenInclude(s => s.User)
                .Include(r => r.RegistrationSession).ThenInclude(rs => rs.Professor).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);
            if (request == null)
            {
                return BadRequest("Invalid request ID");
            }

            //overwrite logic - remove old files of this type for this request
            var oldFile = await _context.Files
                .FirstOrDefaultAsync(f => f.RequestId == requestId && f.FileType == fileType);
            if (oldFile != null)
            {
                if(System.IO.File.Exists(oldFile.FilePath))
                    System.IO.File.Delete(oldFile.FilePath);

                _context.Files.Remove(oldFile);
                await _context.SaveChangesAsync();
            }

            var studentUsername = request.Student?.User?.Username ?? "unknownStudent";
            var profName = request.RegistrationSession?.Professor?.User?.Username ?? "unknownProf";
            var extension = Path.GetExtension(file.FileName);
            var systemFileName = $"{studentUsername}_{profName}_{fileType}{extension}";
            var uniqueFileName = $"{Guid.NewGuid()}_{systemFileName}";

            var uploadFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var filePath = Path.Combine(uploadFolder, uniqueFileName);

            if (file.Length > 50 * 1024 * 1024)
            {
                return BadRequest("File size exceeds 50MB");
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var newFile = new FileUpload
            {
                RequestId = requestId,
                UploadedBy = uploadedBy,
                FileName = systemFileName,
                FilePath = filePath,
                FileType = fileType,
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

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            string contentType = extension switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".doc" => "application/msword",
                _ => "application/octet-stream"
            };

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



        // Get: api/fileupload/by-professor/{professorId}
        [HttpGet("by-professor/{professorId}")]
        public async Task<ActionResult<IEnumerable<ProfessorRequestFilesDto>>> GetFilesByProfessor(int professorId)
        {
            var requests = await _context.RegistrationRequests
                .Include(r => r.Student).ThenInclude(s => s.User)
                .Include(r => r.RegistrationSession)
                .Where(r => r.RegistrationSession.ProfessorId == professorId)
                .ToListAsync();

            var result = new List<ProfessorRequestFilesDto>();

            foreach (var req in requests)
            {
                var files = await _context.Files
                    .Where(f => f.RequestId == req.Id)
                    .ToListAsync();

                var filesByType = files
                    .GroupBy(f => f.FileType)
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderByDescending(f => f.UploadedDate).Select(f => new FileMetadataDto
                        {
                            Id = f.Id,
                            FileName = f.FileName,
                            UploadedBy = f.UploadedBy,
                            UploadedDate = f.UploadedDate,
                            FileType = f.FileType
                        }).FirstOrDefault()
                    );

                result.Add(new ProfessorRequestFilesDto
                {
                    RequestId = req.Id,
                    Student = new StudentInfoDto
                    {
                        Id = req.Student?.UserId,
                        Username = req.Student?.User?.Username,
                        StudentNumber = req.Student?.StudentNumber
                    },
                    Files = filesByType
                });
            }

            return Ok(result);
        }


        // Get: api/fileupload/by-request/{requestId}
        [HttpGet("by-request/{requestId}")]
        public async Task<ActionResult<RequestFilesDto>> GetFilesByRequest(int requestId)
        {
            var request = await _context.RegistrationRequests.FindAsync(requestId);
            if (request == null)
                return NotFound();

            var files = await _context.Files
                .Where(f => f.RequestId == requestId)
                .ToListAsync();

            //group files by FileType, get latest per type
            var filesByType = files
                .GroupBy(f => f.FileType)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(f => f.UploadedDate).Select(f => new FileMetadataDto
                    {
                        Id = f.Id,
                        FileName = f.FileName,
                        UploadedBy = f.UploadedBy,
                        UploadedDate = f.UploadedDate,
                        FileType = f.FileType
                    }).FirstOrDefault()
                );

            //ensure all expected types are present in the dict
            var expectedTypes = new[] { "academic-default", "academic-signed", "progress" };
            foreach (var type in expectedTypes)
            {
                if (!filesByType.ContainsKey(type))
                    filesByType[type] = null;
            }

            var result = new RequestFilesDto
            {
                RequestId = requestId,
                Files = filesByType
            };

            return Ok(result);
        }
    }
}
