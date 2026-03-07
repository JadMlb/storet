import { Injectable, signal } from '@angular/core';
import { ActionType, Service } from './service';
import { ItemMetadataType } from '../types/ItemType';
import { Pagination } from '../types/PaginatedResponse';

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
    this.pagination.set ({previous: response.previous, next: response.next});
    super.handleSuccess (actionType, response.data);
  }
}