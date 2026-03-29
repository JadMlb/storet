using AutoMapper;
using Storet.Core.Authorization;

namespace Storet.Core.Mappers;

public class CurrentUserResolver<TSource, TDestination> : IValueResolver<TSource, TDestination, Guid>
{
	private readonly ICurrentUser user;
	
	public CurrentUserResolver (ICurrentUser user)
	{
		this.user = user;
	}
	
	public Guid Resolve (TSource source, TDestination destination, Guid destMember, ResolutionContext context)
	{
		return user.Id;
	}
}