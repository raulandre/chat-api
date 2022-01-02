using System.ComponentModel.DataAnnotations;

namespace Chat.Api.Models;

class Message
{
    [Key]
    public int Id { get; set; }
    public string Username { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public Message(string username, string content)
    {
        Username = username;
        Content = content;
        Timestamp = DateTime.UtcNow;
    }
}