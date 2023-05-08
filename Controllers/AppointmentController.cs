using Doctor.BLL.Interface;
using Doctor.DataAcsess;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Data;
using NPOI.SS.UserModel;
using DoctorWebApi.Helper;
using NPOI.XSSF.UserModel;

namespace DoctorWebApi.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private IExportUtility _exportUtility { get; set; }

        public AppointmentController(IAppointmentService appointmentService, IExportUtility exportUtility)
        {
            _appointmentService = appointmentService;
            _exportUtility = exportUtility;
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

        // GET: api/Appointment/dates
        [HttpGet("dates")]
        public async Task<IActionResult> GetAllAppointmentsDates()
        {
            var datesList = _appointmentService.GetApointmentDateList();
            return Ok(datesList);
        }

        // POST api/Appointment/save
        [HttpPost]
        [Route("save")]
        public async Task<IActionResult> SaveData(AppointmentDTO model) {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var appointment = await _appointmentService.Add(model);

            return Ok(appointment);
        }


        // GET: api/Appointment/pagedApps/id
        [HttpGet("pagedApps/{id}")]
        public async Task<IActionResult> GetApps([FromQuery] AppointParams appParams, string id)
        {
            var appList = await _appointmentService.GetUserAppoints(appParams, id);
            if (appList == null)
            {
                return BadRequest("User has no role");
            }
            var responce = new PaginationHeader<AppointmentDTOPage>(appList, appParams.PageNumber, appParams.PageSize, appList.TotalCount);
            return Ok(responce);
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
            try
            {
                AppointmentDTO response = new AppointmentDTO();
                response = await _appointmentService.GetDetailsById(id);
                return Ok(response);
            }
            catch 
            {
                return BadRequest("Exceptional error!");
            }

        }


        // GET api/Appointment/GetCalendarDataById/id
        [HttpGet]
        [Route("GetReport/{id}")]
        public async Task<DataTable> GetReport(string id, DateTime startDate, DateTime endDate)
        {
            try
            {
                var response = await _appointmentService.GetAppointmentsReview(id, startDate, endDate);
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(ReportAppointment)); 
                DataTable table = new DataTable();

                foreach (PropertyDescriptor prop in properties)
                        table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);  
                
                foreach (ReportAppointment item in response)
                {
                    DataRow row = table.NewRow();
                    foreach (PropertyDescriptor prop in properties)
                          row[prop.Name] = prop.GetValue(item) ?? DBNull.Value; 
                    table.Rows.Add(row);    
                }

                return table;
            }
            catch (Exception ex) 
            {
                return null;
            }

        }


        [HttpGet]
        [Route("LoadReport/{id}")]
        public async Task<IActionResult> LoadReport(string id, DateTime startDate, DateTime endDate)
        {
            var response = await _appointmentService.GetAppointmentsReview(id, startDate, endDate);
            IWorkbook workbook = _exportUtility.WriteExcelWithNPOI(response, "xlsx");
            string contentType = "";
            MemoryStream tempStream = null;
            MemoryStream stream = null;

            try
            {
                tempStream = new MemoryStream();
                workbook.Write(tempStream, true);
                var byteArray = tempStream.ToArray();
                stream = new MemoryStream();
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Seek(0, SeekOrigin.Begin);
                contentType = workbook.GetType() == typeof(XSSFWorkbook) ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/vnd.ms-excel";
                return File(
                    fileContents: stream.ToArray(),
                    contentType: contentType,
                    fileDownloadName: "Appointments_Report " + DateTime.Now.ToString() + ((workbook.GetType() == typeof(XSSFWorkbook)) ? ".xlsx" : "xls"));
            }
            finally
            {
                if (tempStream != null) tempStream.Dispose();
                if (stream != null) stream.Dispose();
            }

        }

        // PUT api/Appointment/Edit/id
        [HttpPut]
        [Route("Edit/{id}")]
        public async Task<IActionResult> EditAppointmentById(int id, [FromBody] AppointmentDTO appointmentDTO)
        {
            var appointmentToUpdate = await _appointmentService.EditAppointmentById(id, appointmentDTO);
            if (appointmentToUpdate == null)
            {
                return NotFound($"Appointment with Id = {id} not found");
            }

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
