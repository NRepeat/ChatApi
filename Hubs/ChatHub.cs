using Microsoft.AspNetCore.SignalR;
using WebApi.Models;




namespace WebApi.Hubs;

public interface IChatClient
{
    public Task ReciveMessage(string userName, string message);
}


public class ChatHub : Hub<IChatClient>
{
    public async Task JoinChat(UserConnection connection)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);
        await Clients.Group(connection.ChatRoom).ReciveMessage("Client", $"{connection.User} connected");
    }
}


