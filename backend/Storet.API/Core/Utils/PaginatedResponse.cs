namespace Storet.API.Core.Utils;

public class PaginatedResponse<T, K>
{
	public ICollection<T> Data { get; set; } = [];
	public bool HasNext => Next != null;
	public bool HasPrevious => Previous != null;
	public K? Next { get; set; }
	public K? Previous { get; set; }
}