using Doctor.BLL.Extensions;
using Doctor.BLL.Interface;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Doctor.DataAcsess.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace DoctorWebApi.HubSignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IMessageService _messageService;
        public MessageHub(IMessageRepository messageRepository, IMessageService messageService) {
            _messageRepository = messageRepository;
            _messageService = messageService;   
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await _messageService
                           .GetMessagesThread(Context.User.GetUsername(), otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }


        public async Task SendMessage(CreateMessage createParams)
        {
            if (createParams.SenderName == createParams.RecipientName)
            {
                throw new HubException("You cannot send messages to yourself!");
            }

            var sender = await _messageRepository.GetSender(createParams);
            var recepient = await _messageRepository.GetRecepient(createParams);

            if (sender != null && recepient != null)
            {
                var message = new Message
                {
                    Sender = sender,
                    Recipient = recepient,
                    SenderUserName = sender.Name,
                    RecipientUserName = recepient.Name,
                    Content = createParams.Content
                };

                _messageRepository.AddMessage(message);


                if (await _messageRepository.SaveAllAsync())
                {
                    var group = GetGroupName(sender.UserName, recepient.UserName);
                    await Clients.Group(group).SendAsync("NewMessage", message);
                }
            }
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";  
        }
    }
}
