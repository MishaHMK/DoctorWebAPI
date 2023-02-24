using AutoMapper;
using DoctorWebApi.Helpers;
using DoctorWebApi.Models;
using DoctorWebApi.Repositories;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        public async Task<ActionResult<Models.Message>> CreateMessage(string content, string senderId, string recipientId)
        {
            var senderName = _db.Users.Where(u => u.Id == senderId).Select(u => u.Name).First().ToString();
            var recipientName = _db.Users.Where(u => u.Id == recipientId).Select(u => u.Name).First().ToString();

            if (senderName == recipientName)
            {
                return BadRequest("You cannot send messages to yourself!");
            }

            var sender = await _db.Users.SingleOrDefaultAsync(x => x.Name == senderName);
            var recepient = await _db.Users.SingleOrDefaultAsync(x => x.Name == recipientName);

            if(recepient == null) return NotFound("No Recepient");

            var message = new Models.Message
            {
                Sender = sender,
                Recipient = recepient,
                SenderUserName = sender.Name,
                RecipientUserName = recepient.Name,
                Content = content
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDTO>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser([FromQuery] MessageParams messageParams, string userId)
        {
            var messages = await _messageRepository.GetMessagesForUser(messageParams, userId);

            var responce = new PaginationHeader<MessageDTO>(messages, messages.CurrentPage, messages.PageSize, messages.TotalCount);

            return Ok(responce);
        }

        [HttpGet("thread/{un_send}/{un_rec}")]
        public async Task<ActionResult<PagedList<MessageDTO>>> GetMessagesForUser(string un_send, string un_rec)
        {

            var responce = await _messageRepository.GetMessageThread(un_send, un_rec);

            return Ok(responce);
        }
    }
}
