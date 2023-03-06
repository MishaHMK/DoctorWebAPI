using Doctor.BLL.Interface;
using Doctor.DataAcsess;
using Doctor.DataAcsess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ApplicationDbContext _db;

        public AppointmentController(IAppointmentService appointmentService, ApplicationDbContext db)
        {
            _db = db;
            _appointmentService = appointmentService;
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

            var appointment = _appointmentService.Add(model);

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
               response = await _appointmentService.GetDetailsById(id);
               return Ok(response);
            }
            catch 
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
            var appointmentToUpdate = await _appointmentService.ApproveAppointmentById(id, status);   

            if (appointmentToUpdate == null)
            {
                return NotFound();
            }

            return Ok(appointmentToUpdate);
        }

        // DELETE api/Appointment/Delete/id
        [HttpDelete]
        [Route("Delete/{id}")]
        public async Task<IActionResult> DeleteAppointmentById(int id)
        {
            await _appointmentService.DeleteAppointmentById(id);

            return Ok("Removed");

        }
    }
}
