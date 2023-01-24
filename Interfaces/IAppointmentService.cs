using DoctorWebApi.Models;

namespace DoctorWebApi.Interfaces
{
    public interface IAppointmentService
    {
        public List<Doctor> GetDoctorList();
        public List<Patient> GetPatientList();
    }
}
