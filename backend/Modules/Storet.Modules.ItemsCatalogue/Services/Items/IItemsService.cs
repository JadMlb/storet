using Storet.Core.Service;
using Storet.Modules.ItemsCatalogue.Contracts.Items;

namespace Storet.Modules.ItemsCatalogue.Services.Items;

public interface IItemsService : IPaginatedCrudService<ItemResponse, ItemResponseDetails, ItemInsertRequest, ItemUpdateRequest, Guid, string>
{
	public Task<IEnumerable<ItemResponse>> GetAllComponentsAsync ();
	public Task<ItemResponse?> CheckIfExistsAndGetMetadataAsync (Guid itemId);
	public Task<Dictionary<Guid, ItemResponse>?> GetAllFromListAsync (IEnumerable<Guid> itemIds);
	public Task<Dictionary<Guid, ItemResponseWithUnit>> GetAllFromListWithUnitAsync (IEnumerable<Guid> ids);
}