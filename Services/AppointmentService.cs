using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DoctorWebApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _db;
        public AppointmentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> AddOrUpdate(AppointmentDTO model)
        {
            var startDate = Convert.ToDateTime(model.StartDate);
            var endDate = Convert.ToDateTime(model.StartDate).AddMinutes(Convert.ToDouble(60));
            var patient = _db.Users.FirstOrDefault(u => u.Id == model.PatientId);
            var doctor = _db.Users.FirstOrDefault(u => u.Id == model.DoctorId);

            if (model != null && model.Id > 0){ //update

                var appointment = _db.Appointments.FirstOrDefault(x => x.Id == model.Id);
                appointment.Title = model.Title;
                appointment.Description = model.Description;
                appointment.StartDate = startDate;
                appointment.EndDate = endDate;
                appointment.Duration = 60;
                appointment.DoctorId = model.DoctorId;
                appointment.PatientId = model.PatientId;
                appointment.IsApproved = false;
                appointment.AdminId = model.AdminId;
                await _db.SaveChangesAsync();
                return 1;

            }
            else //create
            {
                Appointment appointment = new Appointment()
                {
                    Title = model.Title,
                    Description = model.Description,
                    StartDate = startDate,
                    EndDate = endDate,
                    Duration = 60,
                    DoctorId = model.DoctorId,
                    PatientId = model.PatientId,
                    IsApproved = false,
                    AdminId = model.AdminId
                };

                _db.Appointments.Add(appointment);
                await _db.SaveChangesAsync();
                return 2;
            }
        }

        public List<AppointmentDTO> DoctorEventsById(string doctorId)
        {
            return _db.Appointments.Where(x => x.DoctorId == doctorId).ToList()
                                   .Select(c => new AppointmentDTO()
                                   {
                                       Id= c.Id,    
                                       Title= c.Title,
                                       Description= c.Description,  
                                       StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                       EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                       Duration = c.Duration,
                                       IsApproved = c.IsApproved
                                   }).ToList();
                                  
        }

        public List<AppointmentDTO> PatientsEventsById(string patientId, string doctorId)
        {
            return _db.Appointments.Where(x => x.PatientId == patientId && x.DoctorId == doctorId).ToList()
                                  .Select(c => new AppointmentDTO()
                                  {
                                      Id = c.Id,
                                      Title = c.Title,
                                      Description = c.Description,
                                      StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                      EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                      Duration = c.Duration,
                                      IsApproved = c.IsApproved 
                                  }).ToList();
        }


        public AppointmentDTO GetDetailsById(int Id)
        {
            return _db.Appointments.Where(x => x.Id == Id).ToList()
                                  .Select(c => new AppointmentDTO()
                                  {
                                      Id = c.Id,
                                      Title = c.Title,
                                      Description = c.Description,
                                      StartDate = c.StartDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                      EndDate = c.EndDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                      Duration = c.Duration,
                                      IsApproved = c.IsApproved,
                                      PatientId = c.PatientId,
                                      DoctorId= c.DoctorId, 
                                      PatientName = _db.Users.Where(x=>x.Id==c.PatientId).Select(x=>x.Name).FirstOrDefault(),
                                      DoctorName = _db.Users.Where(x => x.Id == c.DoctorId).Select(x => x.Name).FirstOrDefault()
                                  }).SingleOrDefault();
        }

        public List<Doctor> GetDoctorList()
        {
            var doctors = (from user in _db.Users
                           join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                           join roles in _db.Roles.Where(x => x.Name == Roles.Doctor) on userRoles.RoleId equals roles.Id
                           select new Doctor
                           {
                               Id = user.Id,
                               Name = user.Name
                           }
                           
                           ).ToList();

            return doctors;
        }
        public List<Patient> GetPatientList()
        {

            var patinents = (from user in _db.Users
                           join userRoles in _db.UserRoles on user.Id equals userRoles.UserId
                           join roles in _db.Roles.Where(x => x.Name == Roles.Patient) on userRoles.RoleId equals roles.Id
                           select new Patient
                           {
                               Id = user.Id,
                               Name = user.Name
                           }

                           ).ToList();

            return patinents;
        }
    }
}
