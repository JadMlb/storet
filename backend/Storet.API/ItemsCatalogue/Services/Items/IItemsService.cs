using Storet.API.Core.Service;
using Storet.API.ItemsCatalogue.Contracts.Items;

namespace Storet.API.ItemsCatalogue.Services.Items;

public interface IItemsService : IPaginatedCrudService<ItemResponse, ItemResponseDetails, ItemInsertRequest, ItemUpdateRequest, Guid, string>
{}