import { Injectable } from '@angular/core';
import { ItemCompositionType, ItemType } from '../types/ItemType';
import { DetailsService } from './details-service';
import { CategoryMetadataType } from '../types/CategoryType';
import { ActionType } from './service';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';

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
      quantity: response.quantity,
      unit: response.unit,
      categories: response.categories.map ((c: CategoryMetadataType) => c.id),
      components: response.components.map ((c: ItemCompositionType) => ({id: c.id, quantity: c.quantity}))
    } satisfies ItemType;
    
    super.handleSuccess (actionType, mappedItem);
  }
  
  public override updateForm (response: any)
  {
    const mappedResponse = {
      name: response.name,
      description: response.description,
      quantity: response.quantity,
      unit: response.unit,
      categories: [...response.categories]
    };
    
    const components = this.form!.get ("components") as FormArray;
    components.clear();
    
    (response.components as ItemCompositionType[]).forEach (
      component =>
      {
        const formGroup = new FormGroup ({
          id: new FormControl (component.id, [Validators.required]),
          quantity: new FormControl (component.quantity, [Validators.required, Validators.min (1)])
        });
        formGroup.disable();
        components.push (formGroup);
      }
    );
    
    this.form!.disable();
    this.form!.patchValue (mappedResponse);
  }
}
