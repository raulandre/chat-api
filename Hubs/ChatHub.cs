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
        var message = new Message(username, content);

        if(message.Content.StartsWith('/') && message.Content.Contains("clear"))
        {
            foreach(var m in _context.Messages.ToList())
                _context.Messages.Remove(m);
            await _context.SaveChangesAsync();
            await Clients.All.SendAsync("AllMessages",_context.Messages.ToList());
            return;
        }

        if(_context.Messages.Count() >= 18)
            foreach(var m in _context.Messages.ToList())
                _context.Messages.Remove(m);

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        await Clients.All.SendAsync("NewMessage", message);
    }
}