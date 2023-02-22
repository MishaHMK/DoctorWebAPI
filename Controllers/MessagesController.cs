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

        public MessagesController(ApplicationDbContext db, IMessageRepository messageRepository) 
        {
            _db = db;
            _messageRepository = messageRepository;   
        }

        [HttpPost]
        public async Task<ActionResult<Models.Message>> CreateMessage(CreateMessageDTO createMessageDTO, string userId)
        {
            var userName = _db.Users.Where(u => u.Id == userId).Select(u => u.Name).First().ToString();

            if (userName == createMessageDTO.RecipientUsername)
            {
                return BadRequest("You cannot send messages to yourself!");
            }

            var sender = await _db.Users.SingleOrDefaultAsync(x => x.Name == userName);
            var recepient = await _db.Users.SingleOrDefaultAsync(x => x.Name == createMessageDTO.RecipientUsername);

            if(recepient == null) return NotFound("No Recepient");

            var message = new Models.Message
            {
                Sender = sender,
                Recipient = recepient,
                SenderUserName = sender.Name,
                RecipientUserName = recepient.Name,
                Content = createMessageDTO.Content  
            };

            _messageRepository.AddMessage(message);

            if(await _messageRepository.SaveAllAsync()) return Ok(message);

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<Models.Message>>> GetMessagesForUser([FromQuery] MessageParams messageParams, string userId)
        {
            var messages = await _messageRepository.GetMessagesForUser(messageParams, userId);

            var responce = new PaginationHeader<Models.Message>(messages, messages.CurrentPage, messages.PageSize, messages.TotalCount);

            return Ok(responce);
        }

        [HttpGet("thread/{un_send}/{un_rec}")]
        public async Task<ActionResult<PagedList<Models.Message>>> GetMessagesForUser(string un_send, string un_rec)
        {

            var responce = await _messageRepository.GetMessageThread(un_send, un_rec);

            return Ok(responce);
        }
    }
}
