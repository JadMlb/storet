using Storet.Core.Service;
using Storet.Modules.ItemsCatalogue.Contracts.Items;

namespace Storet.Modules.ItemsCatalogue.Services.Items;

public interface IItemsService : IPaginatedCrudService<ItemResponse, ItemResponseDetails, ItemInsertRequest, ItemUpdateRequest, Guid, string>, IExistenceCheckable<Guid>, IBulkExistenceCheckable<Guid>
{
	public Task<IEnumerable<ItemResponse>> GetAllComponentsAsync ();
	public Task<Dictionary<Guid, List<Guid>>> GetComponentIdsForItemsAsync (IEnumerable<Guid> itemsIds);
	public Task<Dictionary<Guid, ItemResponseWithUnit>> GetAllFromListWithUnitAsync (IEnumerable<Guid> ids);
}