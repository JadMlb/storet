using System.Net;

namespace Storet.Core.Exceptions;

public sealed class DuplicateKeyException : EntityStoretException
{
	public DuplicateKeyException (string entityName, object entityId) :
		base (HttpStatusCode.Conflict, entityName, entityId, $"{entityName} with ID {StringConverter.ConvertToString (entityId)} already exists")
	{}
}