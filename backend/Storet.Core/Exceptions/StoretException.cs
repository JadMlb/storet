namespace Storet.Core.Exceptions;

public abstract class StoretException : Exception
{
	public string EntityName { get; }
	public object EntityId { get; }
	
	public StoretException (string entityName, object entityId, string message) : base (message)
	{
		EntityName = entityName;
		EntityId = entityId;
	}
}