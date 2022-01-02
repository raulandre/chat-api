using Chat.Api.Data;
using Chat.Api.Hubs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase("RAM Database"));

var app = builder.Build();

app.UseRouting();
app.MapHub<ChatHub>("/chat/hub");

app.Run();
