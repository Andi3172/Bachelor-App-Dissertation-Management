namespace Licenta_app.Server.Data
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
    public class RegistrationRequest
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        //public int ProfessorId { get; set; } --from registration session
        //public Professor Professor { get; set; }
        public int RegistrationSessionId { get; set; }
        public RegistrationSession? RegistrationSession { get; set; } ///in web client i need to get the professor name with a dropdown list of open registration sessions based on start date and end date
        public RequestStatus Status { get; set; }
        public string ProposedTheme { get; set; }
        
        public string StatusJustification { get; set; } 
    }
}