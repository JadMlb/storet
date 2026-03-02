using System.Linq.Expressions;

namespace Storet.API.Utils;

public static class CompositeKeyEntityQueryBuilder
{
	/// <summary>
	/// Builds the fluent LINQ query to filter composite key entities using a list of tuples. Code from https://www.reddit.com/r/dotnet/comments/16oe6nk/comment/k1oz8na.
	/// </summary>
	/// <param name="ids">The list of tuples representing the combinations of ids</param>
	/// <param name="pkPropertyNames">The names of the columns/attributes in the entity that map to the parts of the composite key</param>
	/// <typeparam name="T">The type of the entity to return</typeparam>
	/// <typeparam name="TKey1">The type of the first part of the composite key of the entity</typeparam>
	/// <typeparam name="TKey1">The type of the second part of the composite key of the entity</typeparam>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when <c>pkPropertyNames</c>'s length is not 2
	/// </exception>
	public static IQueryable<T> WhereComposite<T, TKey1, TKey2> (this IQueryable<T> baseQuery, IEnumerable<(TKey1 key1, TKey2 key2)> ids, string[] pkPropertyNames)
	{
		if (pkPropertyNames.Length != 2)
			throw new ArgumentOutOfRangeException (nameof (pkPropertyNames), "Number of PK parameters should be 2");
		
		var parameter = Expression.Parameter (typeof (T));
		var body = ids.Select (
			i => Expression.AndAlso (
				Expression.Equal (
					Expression.Property (parameter, pkPropertyNames[0]),
					Expression.Constant (i.key1)
				),
				Expression.Equal (
					Expression.Property (parameter, pkPropertyNames[1]),
					Expression.Constant (i.key2)
				)
			)
		)
		.Aggregate (Expression.OrElse);
		
		var predicate = Expression.Lambda<Func<T, bool>> (body, parameter);
		
		return baseQuery.Where (predicate);
	}
}