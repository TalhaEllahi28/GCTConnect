using GCTConnect.Models;
using GCTConnect.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Plugins;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Message = GCTConnect.Models.Message;
using ProfanityFilter;
using System.Text;
using System.Linq.Expressions;

public class ChatHub : Hub
{
    private readonly GctConnectContext _context;
    private readonly IUserService _userService;

    // Static dictionary to map ConnectionIds to Users
    private static readonly ConcurrentDictionary<string, User> UserConnections = new();
    private static readonly ConcurrentDictionary<int, List<string>> UserConnectionsMap = new();
    public ChatHub(GctConnectContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }
    public override async Task OnConnectedAsync()
    {
        try
        {
            var claimsIdentity = Context.User?.Identity as ClaimsIdentity;
            if (claimsIdentity?.IsAuthenticated == true)
            {
                var username = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
                var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(email))
                {
                    var user = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Username == username && u.Email == email);

                    if (user != null)
                    {
                        UserConnections[Context.ConnectionId] = user;
                        // Track user connections
                        UserConnectionsMap.AddOrUpdate(user.UserId,
                            new List<string> { Context.ConnectionId },
                            (_, connections) =>
                            {
                                lock (connections)
                                {
                                    if (!connections.Contains(Context.ConnectionId))
                                    {
                                        connections.Add(Context.ConnectionId);
                                    }
                                }
                                return connections;
                            });

                        await SendUnreadAnnouncementsToUser(user.UserId);
                    }
                }
            }
            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OnConnectedAsync error: {ex}");
            await base.OnConnectedAsync();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            foreach (var kvp in UserConnectionsMap)
            {
                lock (kvp.Value)
                {
                    if (kvp.Value.Remove(Context.ConnectionId))
                    {
                        if (kvp.Value.Count == 0)
                        {
                            UserConnectionsMap.TryRemove(kvp.Key, out _);
                        }
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OnDisconnectedAsync error: {ex}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public static async Task SendAnnouncementToMultipleUsers(
        IHubContext<ChatHub> hubContext,
        GctConnectContext dbContext,
        int announcementId,
        List<int> userIds,
        string title,
        string content,
        string priority)
    {


        // Send to connected clients
        foreach (var userId in userIds)
        {
            if (UserConnectionsMap.TryGetValue(userId, out var connections))
            {
                foreach (var connectionId in connections.ToList()) // Use ToList for thread safety
                {
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("receiveannouncement", title, content, priority);
                }
            }
        }
    }
    public async Task SendAnnouncementToUser(int userId, string title, string content, string priority)
    {
        await Clients.User(userId.ToString()).SendAsync("receiveannouncement", title, content, priority);
    }


    // Fix the SendUnreadAnnouncementsToUser method
    public async Task SendUnreadAnnouncementsToUser(int userId)
    {
        var unread = await _context.AnnouncementRecipients
            .Include(ar => ar.Announcement)
            .Where(ar => ar.UserId == userId && ar.IsRead == false)
            .Select(ar => new
            {
                ar.Announcement.Title,
                ar.Announcement.Content,
                Priority = ar.Announcement.Priority == 1 ? "Low" :
                          ar.Announcement.Priority == 2 ? "Medium" : "High"
            })
            .ToListAsync();

        foreach (var ann in unread)
        {
            await Clients.User(userId.ToString())
                .SendAsync("receiveannouncement", ann.Title, ann.Content, ann.Priority);
        }
    }


    public async Task JoinGroup(int identifier, bool isPrivate)
    {
        string groupName;

        if (isPrivate)
        {
            var freind = await _context.Users.FirstOrDefaultAsync(u => u.UserId == identifier);
            if (!UserConnections.TryGetValue(Context.ConnectionId, out var currentUser) && freind != null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "Unauthorized action.");
                return;
            }

            // Generate private group name using sorted user IDs
            groupName = GeneratePrivateGroupName(Convert.ToString(currentUser.UserId), Convert.ToString(identifier));
        }
        else
        {

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == identifier);

            // Public/group chat
            if (group == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", $"Group '{identifier}' does not exist.");
                return;
            }
            groupName = Convert.ToString(identifier) + "_" + group.GroupName;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }


    private string GeneratePrivateGroupName(string userId1, string userId2)
    {
        var ids = new[] { userId1, userId2 };
        Array.Sort(ids);
        return $"private_{ids[0]}_{ids[1]}";
    }
    // In ChatHub class
    public async Task<string> EchoTest(string message)
    {
        Console.WriteLine($"Echo test received: {message}");
        await Clients.Caller.SendAsync("EchoResponse", $"Echo: {message}");
        return $"Processed: {message}";
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

    // Send a message to a group with optional file or audio attachment
    public async Task SendMessage(string chatType, int GroupIdOrRecieverId, string message,
        string? fileUrl = null, string? fileType = null, string? fileName = null, string? audioUrl = null)
    {
        var sender = _userService.GetCurrentUser();
        if (sender == null)
            return;

        string groupName = chatType == "friends"
            ? GeneratePrivateGroupName(Convert.ToString(sender.UserId), Convert.ToString(GroupIdOrRecieverId))
            : $"{GroupIdOrRecieverId}_{(_context.Groups.Find(GroupIdOrRecieverId)?.GroupName ?? "group")}";

        if (GroupIdOrRecieverId == 0 || string.IsNullOrEmpty(chatType) ||
            (string.IsNullOrEmpty(message) && string.IsNullOrEmpty(fileUrl) && string.IsNullOrEmpty(audioUrl)))
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

        // -------- PROFANITY FILTER INTEGRATION --------
        if (!string.IsNullOrWhiteSpace(message))
        {
            var filter = new ProfanityFilter.ProfanityFilter();

            //// Option 1: Reject message entirely
            //if (filter.ContainsProfanity(message))
            //{
            //    await Clients.Caller.SendAsync("ReceiveMessage", "System",
            //        "Your message contains inappropriate language and was not sent.");
            //    return;
            //}

            // Option 2: Or automatically censor message
            message = filter.CensorString(message);
        }
        // ----------------------------------------------

        string senderFullName = $"{user.Name} {user.LastName}".Trim();


        if (chatType == "group")
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == GroupIdOrRecieverId);
            if (group == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System",
                    $"Group Id: '{GroupIdOrRecieverId}' does not exist.");
                return;
            }

            var newMessage = new Message
            {
                GroupId = group.GroupId,
                SenderId = user.UserId,
                Content = message,
                FileUrl = fileUrl,
                FileType = fileType,
                FileName = fileName,
                AudioUrl = audioUrl,
                Timestamp = DateTime.Now
            };
            _context.Messages.Add(newMessage);

            if (group.CourseId != null && fileUrl != null)
            {
                try
                {
                    var newFile = new GCTConnect.Models.File
                    {
                        FileUrl = fileUrl,
                        FileType = fileType,
                        FileName = fileName,
                        CourseId = group.CourseId,
                        UploaderId = sender.UserId,
                        UploadedAt = DateTime.Now
                    };
                    _context.Files.Add(newFile);
                }

                catch (Exception e)
                {
                    Console.WriteLine("Error: ", e);
                }
            }

            await _context.SaveChangesAsync();
            await Clients.Group(groupName)
                .SendAsync("ReceiveMessage", user.Username, message, user.ProfilePic, fileUrl, fileType, fileName, audioUrl);
        }
        else if (chatType == "friends")
        {
            var reciver = await _context.Users.FirstOrDefaultAsync(g => g.UserId == GroupIdOrRecieverId);
            if (reciver == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System",
                    $"Reciver Id: '{GroupIdOrRecieverId}' does not exist.");
                return;
            }

            var newMessage = new Message
            {
                ReceiverId = reciver.UserId,
                SenderId = user.UserId,
                Content = message,
                FileUrl = fileUrl,
                FileType = fileType,
                FileName = fileName,
                AudioUrl = audioUrl,
                Timestamp = DateTime.Now
            };
            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();
            await Clients.Group(groupName)
                .SendAsync("ReceiveMessage", user.Username, message, user.ProfilePic, fileUrl, fileType, fileName, audioUrl);
        }
    }
}



