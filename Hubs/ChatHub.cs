using Chat.Api.Data;
using Chat.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Api.Hubs;

class ChatHub : Hub
{
    private readonly DataContext _context;

    public ChatHub(DataContext context)
    {
        _context = context;
    }

    public async Task GetAllMessages()
    {
        await Clients.Caller.SendAsync("AllMessages", _context.Messages.ToList());
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
            _context.Messages.Remove(_context.Messages.First());

        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
        await Clients.All.SendAsync("NewMessage", message);
    }
}