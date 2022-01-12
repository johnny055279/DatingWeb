using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dating_WebAPI.DTOs;
using Dating_WebAPI.Entities;
using Dating_WebAPI.Extensions;
using Dating_WebAPI.Helpers;
using Dating_WebAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }


        // ---- for signalR ----
        // 正常情況不應該把至些資訊存在DB而是要存在記憶體(Redis)，這裡先存DB日後再轉Redis
        public void AddGroup(Group group)
        {
            _dataContext.Groups.Add(group);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _dataContext.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _dataContext.Groups.Include(n => n.Connections).FirstOrDefaultAsync(n => n.Name == groupName);
        }

        public void RemoveConnection(Connection connection)
        {
            _dataContext.Connections.Remove(connection);
        }
        // ----------------------

        public void AddMessage(Message message)
        {
            _dataContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _dataContext.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _dataContext.Messages
            .Include(n => n.Sender)
            .Include(n => n.Recipient)
            .SingleOrDefaultAsync(n => n.Id == id);
        }

        public async Task<PageList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _dataContext.Messages.OrderByDescending(n => n.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(n => n.RecipientUsername == messageParams.Username && n.RecipientDeleted == false),
                "Outbox" => query.Where(n => n.SenderUsername == messageParams.Username && n.SenderDeleted == false),
                _ => query.Where(n => n.RecipientUsername == messageParams.Username && n.DateRead == null && n.RecipientDeleted == false)
            };

            // messageParams繼承了PaginationParams，所以可以使用其屬性。
            return await PageList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            // 獲取傳輸訊息的雙方資訊
            var messages = await _dataContext.Messages
                 .Where(m => m.Recipient.UserName == currentUserName && m.RecipientDeleted == false
                         && m.Sender.UserName == recipientUserName
                         || m.Recipient.UserName == recipientUserName
                         && m.Sender.UserName == currentUserName && m.SenderDeleted == false
                 ).MarkUnreadAsRead(currentUserName)
                 .OrderBy(m => m.MessageSent)
                 .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                 .ToListAsync();

            // 準備將資訊丟回Controller
            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _dataContext.Groups.Include(n => n.Connections).Where(n => n.Connections.Any(n => n.ConnectionId == connectionId)).FirstOrDefaultAsync();
        }
    }
}