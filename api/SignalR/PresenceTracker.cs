using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dating_WebAPI.SignalR
{
	public class PresenceTracker
	{
		// 創建一個dictionary，裡面儲存username跟connection id，
        // 因為每一個裝置連接後，即使是同一個人，connection id 也會不同，所以用List存起來
		private static readonly Dictionary<string, List<string>> OnlineUsers = new();

		// dictionary is not theard safe, so we need to lock dictionary to prevent dictionary sharing when mutiple user connected
		public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;

            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string> { connectionId });
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;

            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                OnlineUsers[username].Remove(connectionId);

                if(OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);

                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;

            lock(OnlineUsers){
                onlineUsers = OnlineUsers.OrderBy(n => n.Key).Select(n => n.Key).ToArray();
            }

            // 將結果包回Task回傳
            return Task.FromResult(onlineUsers);
        }


        // 為了要通知對方有新的訊息出現，必須先找到ConnectionId
        public Task<List<string>> GetConnectionForUser(string username)
        {
            List<string> connectionId;

            lock (OnlineUsers)
            {
                connectionId = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionId);
        }
	}
}

