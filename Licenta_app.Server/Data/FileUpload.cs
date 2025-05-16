namespace Licenta_app.Server.Data
{
    public class FileUpload
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public RegistrationRequest? RegistrationRequest { get; set; }
        public string UploadedBy { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}