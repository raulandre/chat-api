using Chat.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Chat.Api.Data;

class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    { }

    public DbSet<Message> Messages { get; set; }
}