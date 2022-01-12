using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Extensions;
using Dating_WebAPI.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Dating_WebAPI.SignalR
{
    public class MessageHub : Hub
    {
       

        private readonly IMapper mapper;

        private readonly IUnitOfWork unitOfWork;

        private readonly IHubContext<PresenceHub> presenceHubContext;

        private readonly PresenceTracker presenceTracker;

        public MessageHub(IUnitOfWork unitOfWork, IMapper _mapper,
            IHubContext<PresenceHub> _presenceHubContext, PresenceTracker _presenceTracker)
        {
            this.unitOfWork = unitOfWork;

            this.mapper = _mapper;

            this.presenceHubContext = _presenceHubContext;

            this.presenceTracker = _presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            // create a group for each user and make sure everytime they connected will in the same group, so they can talk each other

            var httpContext = Context.GetHttpContext();

            // get who are you talking to, we can set query string url like ?user=XXX
            var otherUser = httpContext.Request.Query["user"].ToString();

            var groupName = getGroupName(Context.User.GetUserName(), otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            // save it to DB (or redis)
            var group = await AddToGroup(groupName);

            // 因為Group在上一行已經被更新了
            await Clients.Group(groupName).SendAsync("UpdateGroup", group);

            // when a user join a group, send the messageDTO
            var message = await unitOfWork.messageRepository.GetMessageThread(Context.User.GetUserName(), otherUser);

            if (unitOfWork.HasChanges()) await unitOfWork.Complete();

            // 無論是誰呼叫都需要ReceiveMessageThread
            await Clients.Caller.SendAsync("ReceiveMessageThread", message);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();

            await Clients.Group(group.Name).SendAsync("UpdateGroup", group);

            // signal will automatically remove group when a user disconnected
            await base.OnDisconnectedAsync(exception);
        }

        // similar with message controller's create message, but not a http fuction
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUserName();

            if (username == createMessageDto.RecipientUsername.ToLower()) throw new HubException("你不能發送訊息給自己!");

            var sender = await unitOfWork.userRepository.GetUserByUserNameAsync(username);

            var recipient = await unitOfWork.userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) throw new HubException("Not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            // 找到這個聊天的名字
            var groupName = getGroupName(sender.UserName, recipient.UserName);

            var group = await unitOfWork.messageRepository.GetMessageGroup(groupName);

            // 篩選出名字符合的接收者並設定讀取時間
            if (group.Connections.Any(n => n.UserName == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await presenceTracker.GetConnectionForUser(recipient.UserName);

                // 如果是null代表不在同一個group
                if (connections != null)
                {
                    await presenceHubContext.Clients.Clients(connections).SendAsync("NewMessageReceived",
                        new { username = sender.UserName, nickName = sender.NickName });
                }
            }

            unitOfWork.messageRepository.AddMessage(message);

            if (await unitOfWork.Complete())
            {

                await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
            }
            else
            {
                throw new HubException("傳送失敗");
            }
        }

        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await unitOfWork.messageRepository.GetMessageGroup(groupName);

            var connection = new Connection(Context.ConnectionId, Context.User.GetUserName());

            if (group == null)
            {
                group = new Group(groupName);

                unitOfWork.messageRepository.AddGroup(group);
            }

            group.Connections.Add(connection);

            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group");
        }

        private async Task<Group> RemoveFromMessageGroup()
        {
            // 藉由connectionId得到這個connection的group
            var group = await unitOfWork.messageRepository.GetGroupForConnection(Context.ConnectionId);

            var connection = group.Connections.FirstOrDefault(n => n.ConnectionId == Context.ConnectionId);

            unitOfWork.messageRepository.RemoveConnection(connection);

            if (await unitOfWork.Complete()) return group;

            throw new HubException("Failde to remove group");
        }

        private static string getGroupName(string caller, string otherUser)
        {
            // 使用CompareOrdinal(包含字母大小寫的排序)進行文字比較來定義groupName規則
            var stringCoimpare = string.CompareOrdinal(caller, otherUser) < 0;

            return stringCoimpare ? $"{caller}-{otherUser}" : $"{otherUser}-{caller}";

        }
    }
}

