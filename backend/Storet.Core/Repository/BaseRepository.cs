using Microsoft.EntityFrameworkCore;

namespace Storet.Core.Repository;

public abstract class BaseRepository<T> where T : DbContext
{
	protected readonly T context;

	public BaseRepository (T context)
	{
		this.context = context;
	}
}