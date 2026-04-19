using Comms_Server;
using Comms_Server.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AttachCommsDatabase(builder.Configuration);
builder.Services.AddCommsServices(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var serviceProvider = scope.ServiceProvider;

	var db = scope.ServiceProvider.GetRequiredService<CommsDbContext>();
	db.Database.Migrate();

	await Database.SeedRolesAsync(serviceProvider);
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
