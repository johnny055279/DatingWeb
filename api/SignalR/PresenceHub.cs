using System;
using System.Threading.Tasks;
using Dating_WebAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Dating_WebAPI.SignalR
{
    [Authorize]
    public class PresenceHub : Hub
	{

        public readonly PresenceTracker presenceTracker;

        public PresenceHub(PresenceTracker _presenceTracker)
        {
            this.presenceTracker = _presenceTracker;
        }

        public override async Task OnConnectedAsync()
        {
            // 這裡微軟並沒有提供目前是誰connect(在線)的方法，因為在多伺服器當中會沒辦法知道這個資訊，
            // 除非使用redis等先把資料存起來，這裡先使用PresenceTracker將已經上線的人存在字典裡。
            var isOline = await presenceTracker.UserConnected(Context.User.GetUserName(), Context.ConnectionId);

            if (isOline)
            {
                await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUserName());
            }

            var currentUsers = await presenceTracker.GetOnlineUsers();

            await Clients.Caller.SendAsync("GetOnlineUsers", currentUsers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline = await presenceTracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);

            if (isOffline)
            {
                await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());
            }

            // 如果產生Exception就將其丟回原本的virtual class做事
            await base.OnDisconnectedAsync(exception);
        }

        
    }
}

