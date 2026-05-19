import { Injectable } from '@angular/core';
import { ActionType, GetPathProps, Service } from '../../../shared/services/data/service';
import { calculateTotalStockFromArray, ItemStock } from '../models/Stock';

@Injectable ({
  providedIn: 'root',
})
export class InventoryService extends Service<ItemStock>
{
  constructor ()
  {
    super ("inventory");
  }

  getInventories (itemsIds: string[]) : void
  {
    const returnedItems = itemsIds.join (",");
    if (!returnedItems)
      return;

    const params: GetPathProps["params"] = {items: returnedItems};

		this.get ({params});
  }

  override handleSuccess (actionType: ActionType, response: ItemStock | null): void
  {
    if (!response)
      super.handleSuccess (actionType, response);
    else
    {
      const totalStockPerItem = Object.fromEntries (
        Object.entries (response)
              .map (
                e => [
                  e[0],
                  [calculateTotalStockFromArray (e[1])]
                ]
              )
      );
      super.handleSuccess (actionType, totalStockPerItem);
    }
  }
}
