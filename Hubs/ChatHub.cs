using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Specialized;
using System.Text.Json;
using WebApi.Models;
namespace WebApi.Hubs;

public interface IChatClient
{
    public Task ReceiveAdminMessage(string userName, string message);
    public Task ReceiveClientMessage(string userName, string message);
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
        await Clients.Group(connection.ChatRoom).ReceiveAdminMessage("Admin", connection.ChatRoom);
    }
    public async Task SendMessage(string message)
    {
        var stringConnection = await _cashe.GetAsync(Context.ConnectionId);
        var connection =  JsonSerializer.Deserialize<UserConnection>(stringConnection);
        Console.WriteLine(connection);
        if(connection is not null)
        {
            await Clients.Group(connection.ChatRoom).ReceiveClientMessage(connection.User, message);
        }
  
    }
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var stringConnection = await _cashe.GetAsync(Context.ConnectionId);
        var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);
        if (connection is not null)
        {
            await _cashe.RemoveAsync(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoom);
                 await Clients.Group(connection.ChatRoom).ReceiveAdminMessage("Admin", $"{connection.User} disconnected");
        }
        await base.OnDisconnectedAsync(exception);
    }
}



