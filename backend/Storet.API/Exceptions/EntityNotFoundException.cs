namespace Storet.API.Exceptions;

public class EntityNotFoundException : Exception
{
	public string EntityName { get; }
	public object EntityId { get; }

	public EntityNotFoundException (string entityName, object entityId) : base ($"{entityName} with ID {ConvertToString (entityId)} was not found")
	{
		EntityName = entityName;
		EntityId = entityId;
	}
	
	private static string? ConvertToString (object o)
	{
		return o switch
		{
			List<int> list => string.Join (",", list.Select (i => i.ToString())),
			_ => o.ToString()
		};
	}
}
