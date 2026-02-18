using Microsoft.EntityFrameworkCore;
using Storet.API.Data;
using Storet.API.Repositories.Categories;
using Storet.API.Services.Categories;

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

#region DI
#region Categories
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
#endregion Categories
#endregion DI

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseHttpsRedirection();