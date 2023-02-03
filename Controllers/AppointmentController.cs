using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly string loginUserId;
        private readonly ApplicationDbContext _db;

        public AppointmentController(IAppointmentService appointmentService, ApplicationDbContext db)
        {
            _appointmentService = appointmentService;
            _db = db;
        }

        // GET: api/Appointment/doctors
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctorList = _appointmentService.GetDoctorList();
            return Ok(doctorList);
        }

        // GET: api/Appointment/patients
        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patientList = _appointmentService.GetPatientList();
            return Ok(patientList);
        }

        // POST api/Appointment/save
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> SaveData([FromBody] AppointmentDTO model) {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var startDate = Convert.ToDateTime(model.StartDate);
            var endDate = Convert.ToDateTime(model.StartDate).AddMinutes(Convert.ToDouble(60));
            var patient = _db.Users.FirstOrDefault(u => u.Id == model.PatientId);
            var doctor = _db.Users.FirstOrDefault(u => u.Id == model.DoctorId);

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

            await _appointmentService.Add(appointment);

            return Ok(appointment);
        }


        // GET api/Appointment/GetCalendarData
        [HttpGet]
        [Route("GetCalendarData")]
        public async Task<IActionResult> GetCalendarData(string role, string? doctorId = " ", string? patientId = " ")
        {
           List<AppointmentDTO> response = new List<AppointmentDTO>();
            try
            {
                if (role == Roles.Patient)
                {
                    response = _appointmentService.PatientsEventsById(patientId, doctorId);
                    return Ok(response);
                }
                else if (role == Roles.Doctor)
                {
                    response = _appointmentService.DoctorEventsById(doctorId);
                    return Ok(response);
                }
                else
                {
                    response = _appointmentService.DoctorEventsById(doctorId);
                    return Ok(response);
                }
            }
            catch (Exception e)
            {
                return BadRequest("Exceptional error!");
            }

        }



        // GET api/Appointment/GetCalendarDataById/id
        [HttpGet]
        [Route("GetCalendarDataById/{id}")]
        public async Task<IActionResult> GetCalendarDataById(int id)
        {
            AppointmentDTO response = new AppointmentDTO();
            try
            {
               response = _appointmentService.GetDetailsById(id);
               return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest("Exceptional error!");
            }

        }

        // PUT api/Appointment/Edit/id
        [HttpPut]
        [Route("Edit/{id}")]
        public async Task<IActionResult> EditAppointmentById(int id, [FromBody] AppointmentDTO appointmentDTO)
        {
            var appointmentToUpdate = _db.Appointments.FirstOrDefault(x => x.Id == appointmentDTO.Id);
            var startDate = Convert.ToDateTime(appointmentDTO.StartDate);
            var endDate = Convert.ToDateTime(appointmentDTO.StartDate).AddMinutes(Convert.ToDouble(60));
            if (appointmentToUpdate == null)
            {
                return NotFound($"Appointment with Id = {id} not found");
            }
            appointmentToUpdate.Title = appointmentDTO.Title;
            appointmentToUpdate.Description = appointmentDTO.Description;
            appointmentToUpdate.StartDate = startDate;
            appointmentToUpdate.EndDate = endDate;
            appointmentToUpdate.Duration = 60;
            appointmentToUpdate.DoctorId = appointmentDTO.DoctorId;
            appointmentToUpdate.PatientId = appointmentDTO.PatientId;
            appointmentToUpdate.IsApproved = false;
            appointmentToUpdate.AdminId = appointmentDTO.AdminId;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PUT api/Appointment/Edit/id
        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAppointmentById(int id)
        {
            var appointmentToDelete = _db.Appointments.FirstOrDefault(x => x.Id == id);
            _db.Appointments.Remove(appointmentToDelete);
            _db.SaveChanges();
            return Ok("Removed");

        }
    }
}
