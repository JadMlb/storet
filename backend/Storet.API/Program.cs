using Storet.API.ItemsCatalogue.Setup;

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

builder.Services.AddItemsCatalogueModule (connectionString);

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