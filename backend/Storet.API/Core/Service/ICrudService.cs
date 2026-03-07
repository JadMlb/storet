namespace Storet.API.Core.Service;

public interface ICrudService<TRetrieveModel, TRetrieveSingleModel, TInsertModel, TUpdateModel, TKey> : IRetrievable<TRetrieveModel, TRetrieveSingleModel, TKey>, IInsertable<TRetrieveSingleModel, TInsertModel>, IUpdatable<TRetrieveSingleModel, TKey, TUpdateModel>, IDeletable<TKey>
{}