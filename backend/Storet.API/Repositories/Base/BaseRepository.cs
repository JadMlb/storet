using Storet.API.Data;

namespace Storet.API.Repositories.Base;

public abstract class BaseRepository
{
	protected readonly StoretDbContext context;

	public BaseRepository (StoretDbContext context)
	{
		this.context = context;
	}
}