namespace Storet.Core.Exceptions;

public class EntityNotFoundException : StoretException
{
	public EntityNotFoundException (string entityName, object entityId) :
		base (entityName, entityId, $"{entityName} with ID {StringConverter.ConvertToString (entityId)} was not found")
	{}
}
