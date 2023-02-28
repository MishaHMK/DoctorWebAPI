using Doctor.DataAcsess.Entities;

namespace DoctorWebApi.Interfaces
{
    public interface IAppointmentService
    {
        public List<Doctor.DataAcsess.Entities.Doctor> GetDoctorList();
        public List<Patient> GetPatientList();
        public Task Add(Appointment model);
        public List<AppointmentDTO> DoctorEventsById(string doctorId);
        public List<AppointmentDTO> PatientsEventsById(string patientId, string doctorId);
        public AppointmentDTO GetDetailsById(int Id);
    }
}
