using Storet.API.Contracts.Items;
using Storet.API.Services.Base;

namespace Storet.API.Services.Items;

public interface IItemsService : IPaginatedCrudService<ItemResponse, ItemResponseWithCategories, ItemInsertRequest, ItemUpdateRequest, Guid, string>
{}