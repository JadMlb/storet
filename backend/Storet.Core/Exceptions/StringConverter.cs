namespace Storet.Core.Exceptions;

public static class StringConverter
{
	public static string? ConvertToString (object o)
	{
		return o switch
		{
			IEnumerable<int> list => string.Join (",", list.Select (i => i.ToString())),
			IEnumerable<Guid> list => string.Join (",", list.Select (i => i.ToString())),
			_ => o.ToString()
		};
	}
}