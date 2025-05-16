namespace Licenta_app.Server.Data
{
    public class RegistrationSession
    {
        public int Id { get; set; }
        public int ProfessorId { get; set; }
        public Professor? Professor { get; set; }
        public int MaxStudents { get; set; } = 5;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}