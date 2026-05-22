using System.Net;

namespace Storet.Core.Exceptions;

public sealed class EntityDependencyException : EntityStoretException
{
	public EntityDependencyException (string entityName, object entityId) :
		base (
			HttpStatusCode.Conflict,
			entityName,
			entityId,
			$"Cannot delete {entityName} with ID {StringConverter.ConvertToString (entityId)} because others depend on it"
		)
	{}
}