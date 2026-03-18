import { Injectable, signal } from '@angular/core';
import { ActionType, Service } from './service';
import { ItemMetadataType } from '../types/ItemType';
import { PaginatedResponse, Pagination } from '../types/PaginatedResponse';

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