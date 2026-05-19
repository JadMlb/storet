import { Injectable } from '@angular/core';
import { ActionType, Service } from '../../../shared/services/data/service';
import { StockWithBounds } from '../models/Stock';

@Injectable({
  providedIn: 'root',
})
export class InventoryDetailsService extends Service<StockWithBounds[]>
{
  constructor ()
  {
    super ("inventory");
  }

  public override handleSuccess (actionType: ActionType, response: StockWithBounds[] | null): void
  {
    if (actionType === "PUT" && response && !Array.isArray (response))
      super.handleSuccess (actionType, [response as StockWithBounds]);
    else
      super.handleSuccess (actionType, response);
  }
}
