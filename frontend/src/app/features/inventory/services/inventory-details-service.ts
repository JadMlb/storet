import { Injectable } from '@angular/core';
import { Service } from '../../../shared/services/service';
import { Stock } from '../models/Stock';

@Injectable({
  providedIn: 'root',
})
export class InventoryDetailsService extends Service<Stock[]>
{
  constructor ()
  {
    super ("inventory");
  }
}
