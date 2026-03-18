using Storet.API.Core.Service;
using Storet.API.ItemsCatalogue.Contracts.Items;
using Storet.API.ItemsCatalogue.Models;

namespace Storet.API.ItemsCatalogue.Services.Items;

public interface IItemsService : IPaginatedCrudService<ItemResponse, ItemResponseDetails, ItemInsertRequest, ItemUpdateRequest, Guid, string>
{
	public Task<IEnumerable<ItemResponse>> GetAllComponentsAsync ();
}