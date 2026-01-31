using Comms_Server;
using Comms_Server.Database;
using Comms_Server.Database.DbContext;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load development env variables
if (builder.Environment.IsDevelopment())
{
	DotNetEnv.Env.Load("../../.env");
}

builder.Services.AttachCommsDatabase();
builder.Services.AddCommsServices();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply any pending database migrations on start up
using (var scope = app.Services.CreateScope())
{
	var serviceProvider = scope.ServiceProvider;

	// Apply pending migrations
	var db = scope.ServiceProvider.GetRequiredService<CommsDbContext>();
	db.Database.Migrate();

	// Seed roles
	await Database.SeedRolesAsync(serviceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
