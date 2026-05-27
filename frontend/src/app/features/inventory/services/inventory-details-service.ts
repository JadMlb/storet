import { Injectable } from '@angular/core';
import { ActionType } from '../../../shared/services/data/service';
import { UpdatableService } from '../../../shared/services/data/updatable-service';
import { StockWithBounds } from '../models/Stock';

@Injectable({
  providedIn: 'root',
})
export class InventoryDetailsService extends UpdatableService<StockWithBounds[]>
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
