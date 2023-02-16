using DoctorWebApi.Helpers;
using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DoctorWebApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        public AppointmentService(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }

        public async Task Add(Appointment appointment)
        {
            await _db.Appointments.AddAsync(appointment);
            await _db.SaveChangesAsync();
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
