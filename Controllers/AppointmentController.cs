using Doctor.DataAcsess;
using Doctor.DataAcsess.Entities;
using DoctorWebApi.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DoctorWebApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AppointmentController(IAppointmentService appointmentService, ApplicationDbContext db, IEmailSender emailSender)
        {
            _appointmentService = appointmentService;
            _db = db;
            _emailSender = emailSender;
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

            await _emailSender.SendEmailAsync(doctor.Email, "Appointment Created", 
                                              $"Your appointment with {patient.Name} has been created and in pending status");
            await _emailSender.SendEmailAsync(patient.Email, "Appointment Created",
                                              $"Your appointment with {doctor.Name} has been created and in pending status");
            await _appointmentService.Add(appointment);

            return Ok(appointment);
        }


        // GET api/Appointment/GetCalendarData
        [HttpGet]
        [Route("GetCalendarData")]
        //[Authorize("Doctor" = "Doctor")]
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
            return Ok(appointmentToUpdate);
        }

        // PATCH  api/Appointment/Approve/id
        [HttpPatch]
        [Route("Approve/{id}/{status}")]
        public async Task<IActionResult> ApproveAppointmentById(int id, bool status) { 
            var appointmentToUpdate = _db.Appointments.FirstOrDefault(x => x.Id == id);

            if (appointmentToUpdate == null)
            {
                return NotFound();
            }

            appointmentToUpdate.IsApproved = status;
            var patient = _db.Users.FirstOrDefault(u => u.Id == appointmentToUpdate.PatientId);
            var doctor = _db.Users.FirstOrDefault(u => u.Id == appointmentToUpdate.DoctorId);


            if(status == true)
            {
                await _emailSender.SendEmailAsync(doctor.Email, "Appointment Approved",
                                           $"You have approved appointment #{appointmentToUpdate.Id} with " +
                                           $"{patient.Name}");
                await _emailSender.SendEmailAsync(patient.Email, "Appointment Approved",
                                                  $"Your appointment #{appointmentToUpdate.Id} with " +
                                                  $"{doctor.Name} has been approved");
            }
            else if (status == false)
            {
                await _emailSender.SendEmailAsync(doctor.Email, "Appointment Cancelled",
                                                            $"You have cancelled appointment #{appointmentToUpdate.Id} with " +
                                                            $"{patient.Name}");
                await _emailSender.SendEmailAsync(patient.Email, "Appointment Cancelled",
                                                  $"Your appointment #{appointmentToUpdate.Id} with " +
                                                  $"{doctor.Name} has been cancelled");
            }

            await _db.SaveChangesAsync();
            return Ok(appointmentToUpdate);
        }

        // DELETE api/Appointment/Delete/id
        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAppointmentById(int id)
        {
            var appointmentToDelete = _db.Appointments.FirstOrDefault(x => x.Id == id);
            _db.Appointments.Remove(appointmentToDelete);

            var patient = _db.Users.FirstOrDefault(u => u.Id == appointmentToDelete.PatientId);
            var doctor = _db.Users.FirstOrDefault(u => u.Id == appointmentToDelete.DoctorId);

            await _emailSender.SendEmailAsync(doctor.Email, "Appointment Deleted",
                                             $"Your appointment #{appointmentToDelete.Id} with " +
                                             $"{patient.Name} has been deleted");
            await _emailSender.SendEmailAsync(patient.Email, "Appointment Deleted",
                                              $"Your appointment #{appointmentToDelete.Id} with " +
                                              $"{doctor.Name} has been deleted");

            _db.SaveChanges();
            return Ok("Removed");

        }
    }
}
