using DoctorWebApi.Helpers;
using DoctorWebApi.Models;

namespace DoctorWebApi.Repositories
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id); 
        Task<PagedList<MessageDTO>> GetMessagesForUser(MessageParams messageParams, string userId);
        Task<IEnumerable<MessageDTO>> GetMessageThread(string currentUserName, string recipientUserName);
        Task<bool> SaveAllAsync();

    }
}
