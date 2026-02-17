namespace Storet.API.Models;

public class Item
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public float BasePrice { get; set; }
	public string BasePriceUnit { get; set; } = string.Empty;
}