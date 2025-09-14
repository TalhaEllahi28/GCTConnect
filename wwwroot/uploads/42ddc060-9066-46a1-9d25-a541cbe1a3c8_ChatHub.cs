using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Claims;

public class ChatHub : Hub
{
    private readonly GctConnectContext _context;
    private readonly IUserService _userService;

    // Static dictionary to map ConnectionIds to Users
    private static readonly ConcurrentDictionary<string, User> UserConnections = new();

    public ChatHub(GctConnectContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // Called when a client connects to the hub
    public override async Task OnConnectedAsync()
    {
        var claimsIdentity = Context.User?.Identity as ClaimsIdentity;
        if (claimsIdentity != null && claimsIdentity.IsAuthenticated)
        {
            var username = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
            var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(email))
            {
                // Fetch the user from the database
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Email == email);
                if (user != null)
                {
                    // Map the ConnectionId to the authenticated user
                    UserConnections[Context.ConnectionId] = user;
                }
            }
        }

        await base.OnConnectedAsync();
    }

    // Called when a client disconnects from the hub
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        UserConnections.TryRemove(Context.ConnectionId, out _);
        await base.OnDisconnectedAsync(exception);
    }

    // Join a specific group
    public async Task JoinGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "Group name cannot be empty.");
            return;
        }

        var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);
        if (group == null)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Group '{groupName}' does not exist.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    // Leave a group
    public async Task LeaveGroup(string groupName)
    {
        if (string.IsNullOrEmpty(groupName))
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "Group name cannot be empty.");
            return;
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    // Send a message to a group
    public async Task SendMessage(string groupName, string message)
    {
        if (string.IsNullOrEmpty(groupName) || string.IsNullOrEmpty(message))
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "Message or group name cannot be empty.");
            return;
        }

        // Retrieve the user from the connection map
        if (!UserConnections.TryGetValue(Context.ConnectionId, out var user))
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", "Unauthorized action. Please log in.");
            return;
        }

        var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupName == groupName);
        if (group == null)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Group '{groupName}' does not exist.");
            return;
        }

        // Save the message to the database
        var newMessage = new Message
        {
            GroupId = group.GroupId,
            SenderId = user.UserId,
            Content = message,
            Timestamp = DateTime.UtcNow
        };
        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        // Broadcast the message to the group along with the sender's profile picture.
        // Note: The client-side code should now expect (user, message, profilePic) as parameters.
        await Clients.Group(groupName).SendAsync("ReceiveMessage", user.Username, message, user.ProfilePic);
    }
}
