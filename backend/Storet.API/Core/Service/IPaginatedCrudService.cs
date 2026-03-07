namespace Storet.API.Core.Service;

public interface IPaginatedCrudService<TRetrieveModel, TRetrieveSingleModel, TInsertModel, TUpdateModel, TKey, TPaginationKey> : IPaginatedRetrievable<TRetrieveModel, TRetrieveSingleModel, TKey, TPaginationKey>, IInsertable<TRetrieveSingleModel, TInsertModel>, IUpdatable<TRetrieveSingleModel, TKey, TUpdateModel>, IDeletable<TKey>
{}