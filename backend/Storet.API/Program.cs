using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Storet.API.Authorization;
using Storet.API.ErrorHandling;
using Storet.Core.Authorization;
using Storet.Modules.Inventory.EventsHandlers;
using Storet.Modules.Inventory.Setup;
using Storet.Modules.ItemsCatalogue.Events;
using Storet.Modules.ItemsCatalogue.Setup;

var builder = WebApplication.CreateBuilder(args);

DotNetEnv.Env.Load();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
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

var authAuthority = Environment.GetEnvironmentVariable ("AUTH") ??
						throw new InvalidOperationException ("Connection string is not configured");

builder.Services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer (
					opt =>
					{
						opt.Authority = authAuthority;
						opt.Audience = "authenticated";
						opt.TokenValidationParameters = new TokenValidationParameters
						{
							ValidateIssuer = true,
							ValidIssuer = authAuthority,
							ValidateAudience = true,
							ValidAudience = "authenticated",
							ValidateLifetime = true
						};
					}
				);

builder.Services.AddTransient<ProblemDetailsFactory, GlobalProblemDetailsFactory>();

builder.Services.AddMediatR (
	cfg =>
	{
		cfg.LicenseKey = Environment.GetEnvironmentVariable ("MEDIATR") ??
							throw new InvalidOperationException ("MediatR key not provided");
		cfg.RegisterServicesFromAssemblies (
			typeof(ItemsCreatedEvent).Assembly,
			typeof(ItemsCreatedEventHandler).Assembly
		);
	}
);

var connectionString = Environment.GetEnvironmentVariable ("CONNECTION_STRING") ??
						throw new InvalidOperationException ("Connection string is not configured");

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddItemsCatalogueModule (connectionString);
builder.Services.AddInventoryModule (connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseCors ("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();