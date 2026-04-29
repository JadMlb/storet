using MediatR;

namespace Storet.Modules.ItemsCatalogue.Events;

public class ItemsDeletedEvent : INotification
{
	public IEnumerable<Guid> ItemIds { get; }
	public Guid UserId { get; }
	
	public ItemsDeletedEvent (IEnumerable<Guid> itemIds, Guid userId)
	{
		ItemIds = itemIds;
		UserId = userId;
	}
}