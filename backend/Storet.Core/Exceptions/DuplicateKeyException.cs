namespace Storet.Core.Exceptions;

public class DuplicateKeyException : StoretException
{
	public DuplicateKeyException (string entityName, object entityId) :
		base (entityName, entityId, $"{entityName} with ID {StringConverter.ConvertToString (entityId)} already exists")
	{}
}