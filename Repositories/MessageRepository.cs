using AutoMapper;
using AutoMapper.QueryableExtensions;
using DoctorWebApi.Helpers;
using DoctorWebApi.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DoctorWebApi.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public MessageRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            _db.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _db.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _db.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams, string userId)
        {
            var query = _db.Messages
                           .OrderByDescending(m => m.MessageSent)
                           .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientId == userId),
                "Outbox" => query.Where(u => u.SenderId == userId),
                _ => query.Where(u => u.RecipientId == userId && u.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDTO>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDTO>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize, query.Count());
        }

        public async Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserId, string recipientId)
        {
            var messages = await _db.Messages
                           .Where(m => m.RecipientId == currentUserId &&
                                  m.RecepientDeleted == false &&
                                  m.SenderId == recipientId ||
                                  m.RecipientId == recipientId &&
                                  m.SenderId == currentUserId && 
                                  m.SenderDeleted == false)
                           .OrderBy(m => m.MessageSent) 
                           .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null &&
                                 m.RecipientId == currentUserId).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow; 
                }

                await _db.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDTO>>(messages);

        }

        public async Task<bool> SaveAllAsync()
        {
            return await _db.SaveChangesAsync() > 0;
        }
    }
}
