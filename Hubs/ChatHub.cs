using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebApi.Models;
namespace WebApi.Hubs;

public interface IChatClient
{
    public Task ReceiveMessage(string userName, string message);
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
        await Clients.Group(connection.ChatRoom).ReceiveMessage("Client", $"{connection.User} connected");
    }
    public async Task SendMessage(string message)
    {
        var stringConnection = await _cashe.GetAsync(Context.ConnectionId);
        var connection =  JsonSerializer.Deserialize<UserConnection>(stringConnection);
        if(connection is not null)
        {
            await Clients.Group(connection.ChatRoom).ReceiveMessage(connection.User, message);
        }
  
    }
}



