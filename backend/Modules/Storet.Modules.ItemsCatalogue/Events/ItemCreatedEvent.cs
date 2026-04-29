using MediatR;

namespace Storet.Modules.ItemsCatalogue.Events;

public class ItemsCreatedEvent : INotification
{
	public IEnumerable<Guid> ItemIds { get; }
	public Guid UserId { get; }
	
	public ItemsCreatedEvent (IEnumerable<Guid> itemIds, Guid userId)
	{
		ItemIds = itemIds;
		UserId = userId;
	}
}