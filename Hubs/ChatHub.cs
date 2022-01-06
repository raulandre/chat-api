using System.Security.Claims;
using Chat.Api.Data;
using Chat.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs;

class ChatHub : Hub
{
    private readonly DataContext _context;
    private static long UserCount = 0;

    public ChatHub(DataContext context)
    {
        _context = context;
    }

    public override Task OnConnectedAsync()
    {
        UserCount++;
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        UserCount--;
        var claim = Context.User.Claims.Where(c => c.Type == ClaimTypes.Name).LastOrDefault();
        if(claim != null)
            Clients.Others.SendAsync("Disconnected", claim.Value);
        Clients.All.SendAsync("UserCount", UserCount);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task GetAllMessages()
    {
        await Clients.All.SendAsync("UserCount", UserCount);
        await Clients.Caller.SendAsync("AllMessages", _context.Messages.ToList().OrderBy(m => m.Timestamp));
    }

    public async Task PostMessage(string username, string content)
    {

        if(username.Length > 20 || content.Length > 999)
            return;

        var message = new Message(username, content);

        if(message.Content.StartsWith('/') && message.Content.Contains("clear"))
        {
            foreach(var m in _context.Messages.ToList())
                _context.Messages.Remove(m);
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("AllMessages",_context.Messages.ToList());
            return;
        }

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        await Clients.All.SendAsync("NewMessage", message);
    }

    public async Task UserConnected(string username)
    {
        var claim = new List<Claim> { new Claim(ClaimTypes.Name, username) };
        Context.User.AddIdentity(new ClaimsIdentity(claim));
        await Clients.Others.SendAsync("Connected", username);
    }

    public async Task UserDisconnected(string username)
    {
        await Clients.Others.SendAsync("Disconnected", username);
    }

    public async Task UsernameChange(string oldUsername, string newUsername)
    {
        var newclaim = new List<Claim> { new Claim(ClaimTypes.Name, newUsername) };
        Context.User.AddIdentity(new ClaimsIdentity(newclaim));

        await Clients.Others.SendAsync("UsernameChange", oldUsername, newUsername);
    }
}