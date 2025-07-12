namespace Licenta_app.Server.Models
{
    public class FileMetadataDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public string FileType { get; set; }
    }

    public class StudentInfoDto
    {
        public int? Id { get; set; }
        public string Username { get; set; }
        public string StudentNumber { get; set; }
    }

    public class ProfessorRequestFilesDto
    {
        public int RequestId { get; set; }
        public StudentInfoDto Student { get; set; }
        public Dictionary<string, FileMetadataDto> Files { get; set; }
    }
}
