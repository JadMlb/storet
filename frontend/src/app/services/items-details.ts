import { Injectable } from '@angular/core';
import { ItemType } from '../types/ItemType';
import { DetailsService } from './details-service';
import { CategoryMetadataType } from '../types/CategoryType';
import { ActionType } from './service';

@Injectable ({
  providedIn: 'root',
})
export class ItemsDetailsService extends DetailsService<ItemType>
{
  constructor ()
  {
    super ("items");
  }
  
  override handleSuccess (actionType: ActionType, response: any): void
  {
    if (response === null)
    {
      super.handleSuccess (actionType, response);
      return;
    }
    
    var mappedItem = {
      id: response.id,
      name: response.name,
      description: response.description,
      categories: response.categories.map ((c: CategoryMetadataType) => c.id)
    } satisfies ItemType;
    
    super.handleSuccess (actionType, mappedItem);
  }
  
  protected override mapResponseToFormData (response: any)
  {
    return {
      name: response.name,
      description: response.description,
      categories: [...response.categories]
    };
  }
}
