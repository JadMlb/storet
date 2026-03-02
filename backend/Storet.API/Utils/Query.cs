namespace Storet.API.Utils;

public class Query<T>
{
	public T? Key { get; set; }
	private int pageSize = 10;
	public int PageSize
	{
		get => pageSize;
		set => pageSize = Math.Min (value, 50);
	}
}