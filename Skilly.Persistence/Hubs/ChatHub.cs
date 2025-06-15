using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class ChatHub : Hub
{
    public static ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

    public ChatHub()
    {
    }
    public async Task SendMessage(string receiverId, string messageContent, string? imageUrl)
    {
        var senderId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(receiverId) && Users.TryGetValue(receiverId, out var receiverConnectionId))
        {
            await Clients.Client(receiverConnectionId)
                .SendAsync("ReceiveMessage", senderId, messageContent, imageUrl);
        }

        if (!string.IsNullOrEmpty(senderId) && Users.TryGetValue(senderId, out var senderConnectionId))
        {
            await Clients.Client(senderConnectionId)
                .SendAsync("ReceiveMessage", senderId, messageContent, imageUrl);
        }

        Console.WriteLine($"Message sent from {senderId} to {receiverId}: {messageContent} - {imageUrl}");
    }
    public override async Task OnConnectedAsync()
    {
        var token = Context.GetHttpContext()?.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token))
        {
            var userId = await GetUserIdFromToken(token);
            if (!string.IsNullOrEmpty(userId))
            {
                Users[userId] = Context.ConnectionId; 
                Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");
            }
            else
            {
                await Clients.Caller.SendAsync("Unauthorized", "Invalid token");
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Unauthorized", "Token not provided");
        }

        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        var user = Users.FirstOrDefault(x => x.Value == Context.ConnectionId);
        if (!string.IsNullOrEmpty(user.Key))
        {
            Users.TryRemove(user.Key, out _);
        }
        return base.OnDisconnectedAsync(exception);
    }

    private async Task<string> GetUserIdFromToken(string token)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        var jsonToken = jwtHandler.ReadToken(token) as JwtSecurityToken;
        var userId = jsonToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return userId;
    }
}
