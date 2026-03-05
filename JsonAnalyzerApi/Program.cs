
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Host.ConfigureServices((_, services) =>
{
    services.Configure<RouteOptions>(o => o.LowercaseUrls = true);
});
var app = builder.Build();

app.MapControllers();

app.Run();

public partial class Program { }
