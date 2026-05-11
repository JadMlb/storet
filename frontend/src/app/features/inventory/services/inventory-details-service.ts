import { Injectable } from '@angular/core';
import { Service } from '../../../shared/services/service';
import { StockWithBounds, Stock } from '../models/Stock';

@Injectable({
  providedIn: 'root',
})
export class InventoryDetailsService extends Service<StockWithBounds[]>
{
  constructor ()
  {
    super ("inventory");
  }
}
