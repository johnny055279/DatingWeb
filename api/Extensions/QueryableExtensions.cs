using System;
using System.Linq;
using Dating_WebAPI.Entities;

namespace Dating_WebAPI.Extensions
{
    public static class QueryableExtensions
    {
        // 獲取任何未讀取的資訊(利用多對多產生的欄位null決定)
        // Tracking the changes we set in the unread messages
        // EF tracks the changes is to get a list of the unread messages before we use projection and then turn the Message entity into a MessageDto
        // To resolve the issue we need to first get a list of the unread messages as the Message entities, then mark them as read, and then go and get the list of messages in the thread.
        public static IQueryable<Message> MarkUnreadAsRead(this IQueryable<Message> query, string currentUsername)
        {
            var unreadMessages = query.Where(m => m.DateRead == null
                && m.RecipientUsername == currentUsername);

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
            }

            return query;
        }
    }
}

