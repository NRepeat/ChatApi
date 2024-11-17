using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Specialized;
using System.Text.Json;
using WebApi.Models;
namespace WebApi.Hubs;




public interface IChatClient
{
    public Task ReceiveAdminMessage(IMessage message);
    public Task ReceiveClientMessage(IMessage message);
}


public class ChatHub : Hub<IChatClient>
{
    private readonly IDistributedCache _cashe;
    public ChatHub(IDistributedCache cashe)
    {
        _cashe = cashe;
    }

    public async Task JoinChat(UserConnection connection)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
        string stringConnection = JsonSerializer.Serialize(connection);
        await _cashe.SetStringAsync(Context.ConnectionId,stringConnection);
        DateTime time = DateTime.Now;
        Guid messageGuuid = Guid.NewGuid();
        Message adminMessage = new Message
        {
            Guid = messageGuuid,
            UserName = connection.User,
            MessageValue = connection.ChatRoom,
            Time = time
        };
        await Clients.Group(connection.ChatRoom).ReceiveAdminMessage(adminMessage);
    }
    public async Task SendMessage(string message)
    {
        var stringConnection = await _cashe.GetAsync(Context.ConnectionId);
        var connection =  JsonSerializer.Deserialize<UserConnection>(stringConnection);
        Console.WriteLine(connection);
        if(connection is not null)
        {
            DateTime time = DateTime.Now;
            Guid messageGuuid = Guid.NewGuid();
            Message messageObj = new Message
            {
                Guid = messageGuuid,
                UserName = "Admin",
                MessageValue = message,
                Time = time
            };
            await Clients.Group(connection.ChatRoom).ReceiveClientMessage(messageObj);
        }
  
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var stringConnection = await _cashe.GetAsync(Context.ConnectionId);
        var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);
        if (connection is not null)
        {
            DateTime time = DateTime.Now;
            Guid messageGuuid = Guid.NewGuid();
            Message messageObj = new Message
            {

                Guid = messageGuuid,
                UserName = "Admin",
                MessageValue = connection.ChatRoom,
                Time = time
            };
            await _cashe.RemoveAsync(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoom);
                 await Clients.Group(connection.ChatRoom).ReceiveAdminMessage(messageObj);
        }
        await base.OnDisconnectedAsync(exception);
    }
}



