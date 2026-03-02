using Storet.API.Contracts.Categories;

namespace Storet.API.Contracts.Items;

public class ItemResponse
{
	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
}