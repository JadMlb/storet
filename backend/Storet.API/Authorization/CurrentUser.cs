using System.Security.Claims;
using Storet.Core.Authorization;

namespace Storet.API.Authorization;

public class CurrentUser : ICurrentUser
{
	private readonly ClaimsPrincipal? user;
	
	private readonly string? userId;
	
	public CurrentUser (IHttpContextAccessor httpContextAccessor)
	{
		user = httpContextAccessor.HttpContext?.User;
		
		userId = user?.FindFirstValue (ClaimTypes.NameIdentifier) ?? user?.FindFirstValue ("sub");
	}
	
	public Guid Id => userId == null ? new Guid() : Guid.Parse (userId);

	public bool IsAuthenticated => user?.Identity?.IsAuthenticated ?? false;
}