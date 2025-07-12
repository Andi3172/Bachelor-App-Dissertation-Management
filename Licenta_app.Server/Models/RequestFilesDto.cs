namespace Licenta_app.Server.Models
{
    public class RequestFilesDto
    {
        public int RequestId { get; set; }
        public Dictionary<string, FileMetadataDto?> Files { get; set; }
    }
}
