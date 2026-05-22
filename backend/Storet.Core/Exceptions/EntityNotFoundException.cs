using System.Net;

namespace Storet.Core.Exceptions;

public sealed class EntityNotFoundException : EntityStoretException
{
	public EntityNotFoundException (string entityName, object entityId) :
		base (HttpStatusCode.NotFound, entityName, entityId, $"{entityName} with ID {StringConverter.ConvertToString (entityId)} was not found")
	{}
}
