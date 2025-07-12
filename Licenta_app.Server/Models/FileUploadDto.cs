namespace Licenta_app.Server.Models
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
        public int RequestId { get; set; }
        public string UploadedBy { get; set; }
        public string FileType { get; set; }
    }
}
