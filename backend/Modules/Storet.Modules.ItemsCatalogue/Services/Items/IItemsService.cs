using Storet.Core.Service;
using Storet.Modules.ItemsCatalogue.Contracts.Items;

namespace Storet.Modules.ItemsCatalogue.Services.Items;

public interface IItemsService : IPaginatedCrudService<ItemResponse, ItemResponseDetails, ItemInsertRequest, ItemUpdateRequest, Guid, string>
{
	public Task<IEnumerable<ItemResponse>> GetAllComponentsAsync ();
}