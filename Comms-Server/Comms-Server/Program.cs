using Comms_Server;
using Comms_Server.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
	configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AttachCommsDatabase(builder.Configuration);
builder.Services.AddCommsServices(builder.Configuration, builder.Environment);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var serviceProvider = scope.ServiceProvider;

	var db = scope.ServiceProvider.GetRequiredService<CommsDbContext>();
	db.Database.Migrate();

	await Database.SeedRolesAsync(serviceProvider);
}

app.UseCommsMiddleware();
app.MapCommsEndpoints();

app.Run();
