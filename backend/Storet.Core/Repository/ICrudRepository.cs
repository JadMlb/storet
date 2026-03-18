namespace Storet.Core.Repository;

public interface ICrudRepository<TModel, TKey> : IRetrievable<TModel, TKey>, IInsertable<TModel>, IUpdatable<TModel, TKey>, IDeletable<TKey>
{}