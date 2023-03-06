using AutoMapper;
using Doctor.DataAcsess;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Doctor.DataAcsess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoctorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public MessagesController(ApplicationDbContext db, IMessageRepository messageRepository, IMapper mapper) 
        {
            _db = db;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Message>> CreateMessage(CreateMessage createParams)
        {
            if (createParams.SenderName == createParams.RecipientName)
            {
                return BadRequest("You cannot send messages to yourself!");
            }

            var sender = await _db.Users.SingleOrDefaultAsync(x => x.Name == createParams.SenderName);
            var recepient = await _db.Users.SingleOrDefaultAsync(x => x.Name == createParams.RecipientName);

            if(recepient == null) return NotFound("No Recepient");

            var message = new Message
            {
                Sender = sender,
                Recipient = recepient,
                SenderUserName = sender.Name,
                RecipientUserName = recepient.Name,
                Content = createParams.Content
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send message");
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams, string userId)
        {
            var messages = await _messageRepository.GetMessagesForUser(messageParams, userId);

            var responce = new PaginationHeader<MessageDTO>(messages, messages.CurrentPage, messages.PageSize, messages.TotalCount);

            return Ok(responce);
        }

        [Authorize]
        [HttpGet("thread/{un_send}/{un_rec}")]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesThread(string un_send, string un_rec)
        {

            var responce = await _messageRepository.GetMessageThread(un_send, un_rec);

            return Ok(responce);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id, string un_send)
        {
            var message = await _messageRepository.GetMessage(id);

            if (message.Sender.Name != un_send && message.Recipient.Name != un_send)
                return Unauthorized();

            if (message.Sender.Name == un_send) message.SenderDeleted = true;

            if (message.Recipient.Name == un_send) message.RecepientDeleted = true;

            if (message.SenderDeleted || message.RecepientDeleted)
                _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }

    }
}
