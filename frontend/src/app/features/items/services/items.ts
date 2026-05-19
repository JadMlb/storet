import { Injectable, signal } from '@angular/core';
import { ActionType, Service } from '../../../shared/services/data/service';
import { ItemMetadataType } from '../models/ItemType';
import { Pagination } from '../../../shared/types/PaginatedResponse';

@Injectable ({
  providedIn: 'root',
})
export class ItemsService extends Service<ItemMetadataType[]>
{
  private readonly pagination = signal<Pagination<string>> ({next: null, previous: null});
  
  constructor ()
  {
    super ("items");
  }
  
  override handleSuccess (actionType: ActionType, response: any) : void
  {
    if (Array.isArray (response))
      super.handleSuccess (actionType, response);
    else
    {
      this.pagination.set ({previous: response.previous, next: response.next});
      super.handleSuccess (actionType, response.data);
    }
  }
}