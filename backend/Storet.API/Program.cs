using Microsoft.EntityFrameworkCore;
using Storet.API.Data;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var connectionString = Environment.GetEnvironmentVariable ("CONNECTION_STRING") ??
						throw new InvalidOperationException ("Connection string is not configured");

builder.Services.AddDbContext<StoretDbContext> (
	options => options.UseNpgsql (connectionString)
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();