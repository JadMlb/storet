namespace Storet.API.ItemsCatalogue.Contracts.ItemsCompositions;

public class ItemCompositionResponse
{
	public Guid? Id { get; set; }
	public string? Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public short Quantity { get; set; }
	public string Unit { get; set; } = string.Empty;
}