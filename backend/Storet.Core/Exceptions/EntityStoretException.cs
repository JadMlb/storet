using System.Net;

namespace Storet.Core.Exceptions;

public abstract class EntityStoretException : StoretException
{
	public string EntityName { get; }
	public object EntityId { get; }
	
	public EntityStoretException (HttpStatusCode statusCode, string entityName, object entityId, string message) : base (statusCode, message)
	{
		EntityName = entityName;
		EntityId = entityId;
	}
}