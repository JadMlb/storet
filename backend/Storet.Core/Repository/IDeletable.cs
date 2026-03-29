namespace Storet.Core.Repository;

public interface IDeletable<TKey>
{
	public Task<bool> DeleteAsync (TKey key, Guid userId);
}