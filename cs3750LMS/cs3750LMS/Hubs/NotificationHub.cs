using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace cs3750LMS.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendMesage(String item, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", item, message);
        }
    }
}
