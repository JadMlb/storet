using Microsoft.EntityFrameworkCore;

namespace Storet.API.Core.Repository;

public abstract class BaseRepository<T> where T : DbContext
{
	protected readonly T context;

	public BaseRepository (T context)
	{
		this.context = context;
	}
}