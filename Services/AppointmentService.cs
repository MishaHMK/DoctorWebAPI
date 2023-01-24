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
        public List<Doctor> GetDoctorList()
        {
            //var doctors = _db.Users
            //     .Join(_db.UserRoles, u => u.Id, ur => ur.UserId)
            //     .Join(_db.Roles.Where(x => x.Name == Roles.Doctor), ur => ur.RoleId, r => r.Id)
            //     .Select(x => new Doctor
            //        {
            //            Id= x.Id,
            //            Name = x.Name
            //        }).ToList();

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
