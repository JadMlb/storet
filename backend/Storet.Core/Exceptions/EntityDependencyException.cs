namespace Storet.Core.Exceptions;

public class EntityDependencyException : StoretException
{
	public EntityDependencyException (string entityName, object entityId) :
		base (
			entityName,
			entityId,
			$"Cannot delete {entityName} with ID {StringConverter.ConvertToString (entityId)} because others depend on it"
		)
	{}
}