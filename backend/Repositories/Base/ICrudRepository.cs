namespace Storet.Backend.Repositories.Base;

public interface ICrudRepository<TModel, TKey> : IRetrievable<TModel, TKey>, IInsertable<TModel>, IUpdatable<TModel, TKey>, IDeletable<TModel, TKey>
{}