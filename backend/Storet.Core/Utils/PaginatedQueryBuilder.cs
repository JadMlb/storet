using System.Linq.Expressions;

namespace Storet.Core.Utils;

public static class PaginatedQueryBuilder
{
	private static BinaryExpression Compare (Expression left, Expression right, bool reverseOrder)
	{
		return reverseOrder switch
		{
			true => Expression.LessThanOrEqual (left, right),
			false => Expression.GreaterThanOrEqual (left, right)
		};
	}
	
	private static Expression<Func<T, bool>> BuildComparator<T> (string propertyName, object value, bool reverseOrder)
	{
		var param = Expression.Parameter (typeof (T), "e");
		var prop = Expression.PropertyOrField (param, propertyName);
		var constant = Expression.Constant (value);
		var body = value switch
		{
			string => Compare (
				Expression.Call (
					prop,
					typeof(string).GetMethod ("CompareTo", [typeof (string)])!,
					constant
				),
				Expression.Constant (0),
				reverseOrder
			),
			_ => Compare (prop, constant, reverseOrder)
		};
		return Expression.Lambda<Func<T, bool>> (body, param);
	}

	public static IQueryable<T> PaginateQuery<T, V> (this IQueryable<T> dbQuery, Query<V> searchQuery, string keyName, bool reverseOrder = false)
	{
		if (searchQuery.PageSize < 1)
			throw new InvalidOperationException ("Invalid query parameters");
		
		var baseQuery = dbQuery;
		if (searchQuery.Key != null)
			baseQuery = baseQuery.Where (BuildComparator<T> (keyName, searchQuery.Key, reverseOrder));
		return baseQuery.Take (searchQuery.PageSize + 1);
	}
}