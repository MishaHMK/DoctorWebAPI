using Doctor.BLL.Interface;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService) 
        {
            _messageService = messageService;
        }

        //[Authorize]
        //[HttpPost]
        //public async Task<IActionResult> CreateMessage(CreateMessage createParams)
        //{
        //    if (createParams.SenderName == createParams.RecipientName)
        //    {
        //        return BadRequest("You cannot send messages to yourself!");
        //    }

        //    if(createParams.RecipientName == null) return NotFound("No Recepient");

        //    var message = await _messageService.CreateMessage(createParams);

        //    if(message != null)
        //    {
        //        await _messageService.SaveAllAsync();
        //        return Ok(message);
        //    }

        //    return BadRequest("Failed to send message");
        //}

       // [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMessage(CreateMessage createParams)
        {
            if (createParams.SenderId == createParams.RecipientId)
            {
                return BadRequest("You cannot send messages to yourself!");
            }

            if (createParams.RecipientId == null) return NotFound("No Recepient");

            var message = await _messageService.CreateMessage(createParams);

            if (message != null)
            {
                await _messageService.SaveAllAsync();
                return Ok(message);
            }

            return BadRequest("Failed to send message");
        }

       // [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser([FromQuery] MessageParams messageParams, string userId)
        {
            var messages = await _messageService.GetMessages(messageParams, userId);

            var responce = new PaginationHeader<MessageDTO>(messages, messages.CurrentPage, messages.PageSize, messages.TotalCount);

            return Ok(responce);
        }

        //[Authorize]
        //[HttpGet("thread/{un_send}/{un_rec}")]
        //public async Task<IActionResult> GetMessagesThread(string un_send, string un_rec)
        //{
        //    var responce = await _messageService.GetMessagesThread(un_send, un_rec);

        //    return Ok(responce);
        //}


       // [Authorize]
        [HttpGet("thread/{id_send}/{id_rec}")]
        public async Task<IActionResult> GetMessagesThread(string id_send, string id_rec)
        {
            var responce = await _messageService.GetMessagesThread(id_send, id_rec);

            return Ok(responce);
        }

        //[Authorize]
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteMessage(int id, string un_send)
        //{     
        //    await _messageService.DeleteMessageAsync(id, un_send);

        //    await _messageService.SaveAllAsync();

        //    return Ok();
        //}

       // [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, string id_send)
        {
            await _messageService.DeleteMessageAsync(id, id_send);

            await _messageService.SaveAllAsync();

            return Ok();
        }

    }
}
