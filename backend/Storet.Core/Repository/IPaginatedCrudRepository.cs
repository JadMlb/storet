namespace Storet.Core.Repository;

public interface IPaginatedCrudRepository<TModel, TKey, TPaginationKey> : IPaginatedRetrievable<TModel, TKey, TPaginationKey>, IInsertable<TModel>, IUpdatable<TModel, TKey>, IDeletable<TKey>
{}