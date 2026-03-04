namespace Storet.API.Contracts.Items;

public class ItemUpdateRequest
{
	public string? Name { get; set; }
	public string? Description { get; set; }
	public List<int>? Categories { get; set; }
}