namespace Storet.Core.Authorization;

public interface ICurrentUser
{
	public Guid Id { get; }
	public bool IsAuthenticated { get; }
}