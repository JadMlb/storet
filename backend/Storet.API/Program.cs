using Microsoft.EntityFrameworkCore;
using Storet.API.Data;
using Storet.API.Mappers;
using Storet.API.Repositories.Categories;
using Storet.API.Repositories.Items;
using Storet.API.Repositories.ItemsCategories;
using Storet.API.Services.Categories;
using Storet.API.Services.Items;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors (
	options =>
	{
		options.AddPolicy (
			"AllowFrontend",
			policy =>
			{
				policy.WithOrigins (builder.Configuration.GetValue<string> ("FrontendUrl") ?? "")
						.AllowAnyHeader()
						.AllowAnyMethod();
			}
		);
	}
);

var connectionString = Environment.GetEnvironmentVariable ("CONNECTION_STRING") ??
						throw new InvalidOperationException ("Connection string is not configured");

builder.Services.AddDbContext<StoretDbContext> (
	options => options.UseNpgsql (connectionString)
);

#region DI
#region Automapper
builder.Services.AddAutoMapper (cfg => cfg.AddProfile<MappingProfile>());
#endregion Automapper
#region Categories
builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
#endregion Categories
#region Items
builder.Services.AddScoped<IItemsCategoriesRepository, ItemsCategoriesRepository>();
builder.Services.AddScoped<IItemsRepository, ItemsRepository>();
builder.Services.AddScoped<IItemsService, ItemsService>();
#endregion Items
#endregion DI

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseCors ("AllowFrontend");

app.UseHttpsRedirection();
app.MapControllers();
app.Run();