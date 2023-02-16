using DoctorWebApi.Helpers;
using DoctorWebApi.Models;
using System.Globalization;

namespace DoctorWebApi.Interfaces
{
    public interface IAppointmentService
    {
        public List<Doctor> GetDoctorList();
        public List<Patient> GetPatientList();
        public Task Add(Appointment model);
        public List<AppointmentDTO> DoctorEventsById(string doctorId);
        public List<AppointmentDTO> PatientsEventsById(string patientId, string doctorId);
        public AppointmentDTO GetDetailsById(int Id);
    }
}
