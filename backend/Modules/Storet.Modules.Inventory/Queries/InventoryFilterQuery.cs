namespace Storet.Modules.Inventory.Queries;

public class InventoryFilterQuery
{
	///<summary>
	/// A comma-separated list of item guids to be used to filter the query
	///</summary>
	public string Items { get; set; } = string.Empty;
}