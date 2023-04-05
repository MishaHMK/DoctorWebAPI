using AutoMapper;
using Doctor.BLL.Interface;
using Doctor.BLL.Services;
using Doctor.DataAcsess;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Doctor.DataAcsess.Interfaces;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Doctor.DataAcsess.Entities.Message>> CreateMessage(CreateMessage createParams)
        {
            if (createParams.SenderName == createParams.RecipientName)
            {
                return BadRequest("You cannot send messages to yourself!");
            }

            if(createParams.RecipientName == null) return NotFound("No Recepient");

            await _messageService.CreateMessage(createParams);

            if(await _messageService.SaveAllAsync()) 
                return Ok("Message sent");

            return BadRequest("Failed to send message");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams, string userId)
        {
            var messages = await _messageService.GetMessages(messageParams, userId);

            var responce = new PaginationHeader<MessageDTO>(messages, messages.CurrentPage, messages.PageSize, messages.TotalCount);

            return Ok(responce);
        }

        [Authorize]
        [HttpGet("thread/{un_send}/{un_rec}")]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesThread(string un_send, string un_rec)
        {

            var responce = await _messageService.GetMessagesThread(un_send, un_rec);

            return Ok(responce);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id, string un_send)
        {     
            await _messageService.DeleteMessageAsync(id, un_send);

            if (await _messageService.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }

    }
}
