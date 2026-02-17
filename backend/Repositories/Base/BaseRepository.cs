using Storet.Backend.Data;

namespace Storet.Backend.Repositories.Base;

public abstract class BaseRepository
{
	protected readonly StoretDbContext context;

	public BaseRepository (StoretDbContext context)
	{
		this.context = context;
	}
}