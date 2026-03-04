using System.Linq.Expressions;

namespace Storet.API.Utils;

public static class PaginatedQueryBuilder
{
	private static Expression<Func<T, bool>> BuildComparator<T> (string propertyName, object value)
	{
		var param = Expression.Parameter (typeof (T), "e");
		var prop = Expression.PropertyOrField (param, propertyName);
		var constant = Expression.Constant (value);
		var body = value switch
		{
			string s => Expression.GreaterThanOrEqual (
				Expression.Call (
					prop,
					typeof(string).GetMethod ("CompareTo", [typeof (string)])!,
					constant
				),
				Expression.Constant (0)
			),
			_ => Expression.GreaterThanOrEqual (prop, constant)
		};
		return Expression.Lambda<Func<T, bool>> (body, param);
	}

	public static IQueryable<T> PaginateQuery<T, V> (this IQueryable<T> dbQuery, Query<V> searchQuery, string keyName)
	{
		if (searchQuery.PageSize < 1)
			throw new InvalidOperationException ("Invalid query parameters");
		
		var baseQuery = dbQuery;
		if (searchQuery.Key != null)
			baseQuery = baseQuery.Where (BuildComparator<T> (keyName, searchQuery.Key));
		return baseQuery.Take (searchQuery.PageSize + 1);
	}
}