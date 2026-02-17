namespace Storet.Backend.Services.Base;

public interface ICrudService<TRetrieveModel, TInsertModel, TUpdateModel, TKey> : IRetrievable<TRetrieveModel, TKey>, IInsertable<TRetrieveModel, TInsertModel>, IUpdatable<TRetrieveModel, TKey, TUpdateModel>, IDeletable<TRetrieveModel, TKey>
{}