namespace DoctorWebApi.Models
{
    public class AppointmentDTO
    {
        public int? Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Duration { get; set; }
        public string? DoctorId { get; set; }
        public string? PatientId { get; set; }
        public bool IsApproved { get; set; }
        public string? AdminId { get; set; }
    }
}
