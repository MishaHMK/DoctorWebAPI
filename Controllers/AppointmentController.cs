using DoctorWebApi.Interfaces;
using DoctorWebApi.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: api/doctors
        [HttpGet("doctors")]
        public async Task<IActionResult> GetDoctors()
        {
            var doctorList = _appointmentService.GetDoctorList();
            return Ok(doctorList);
        }

        // GET: api/patients
        [HttpGet("patients")]
        public async Task<IActionResult> GetPatients()
        {
            var patientList = _appointmentService.GetPatientList();
            return Ok(patientList);
        }

        // POST api/<AppointmentController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<AppointmentController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AppointmentController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
