using Doctor.BLL.Extensions;
using Doctor.BLL.Interface;
using Doctor.DataAcsess.Entities;
using Doctor.DataAcsess.Helpers;
using Doctor.DataAcsess.Interfaces;
using Mailjet.Client.Resources;
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
            //var otherUser = httpContext.Request.Query["user"];
            //var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            //await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            //var messages = await _messageService
            //               .GetMessagesThread(Context.User.GetUsername(), otherUser);

            //await Clients.Group(groupName).SendAsync("GetMessageThread", messages);
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
                var message = new Doctor.DataAcsess.Entities.Message
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

        public async Task RemoveMessage(int id, string un_send)
        {
            var message = await _messageService.GetMessage(id);

            if (message.Sender.Name == un_send) message.SenderDeleted = true;

            if (message.Recipient.Name == un_send) message.RecepientDeleted = true;

            if (message.SenderDeleted || message.RecepientDeleted)
            {
                _messageRepository.DeleteMessage(message);
            }

            if (await _messageRepository.SaveAllAsync())
            {
                await Clients.Caller.SendAsync("DeleteMessage", message.Id);
            }

        }


        public async Task RecieveThread(string sender, string reciever)
        {
            var group = GetGroupName(sender, reciever);
            await Groups.AddToGroupAsync(Context.ConnectionId, group);

            var messages = await _messageService
                          .GetMessagesThread(sender, reciever);

            await Clients.Group(group).SendAsync("RecieveMessageThread", messages);
        }


        public async Task RecieveUnread(string reciever)
        {
            //var httpContext = Context.GetHttpContext();
            MessageParams messageParams = new MessageParams();
            var repsonce = await _messageService.GetMessages(messageParams, reciever);
            await Clients.Caller.SendAsync("ReceiveUnreadCount", repsonce.TotalCount);
            //await Clients.User(reciever).SendAsync("ReceiveUnreadCount", repsonce.TotalCount);
        }

        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";  
        }
    }
}
