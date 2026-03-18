import { Component, computed, inject } from '@angular/core';
import { Suspense } from '../../components/suspense/suspense';
import { AbstractControl, FormArray, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { TextInput } from '../../components/input/input';
import { Combobox } from '../../components/combobox/combobox';
import { ListViewItemDetails } from '../../components/list-view-item-details/list-view-item-details';
import { ListViewDetailsFormLogicBase } from '../../common/ListViewDetailsFormLogicBase';
import { ItemsDetailsService } from '../../services/items-details';
import { CategoriesService } from '../../services/categories';
import { MappingProfile } from '../../common/MappingProfile';
import { Button } from '../../components/button/button';
import { NumberInput } from '../../components/number-input/number-input';
import { ItemsService } from '../../services/items';

function positiveValueValidator (control: AbstractControl) : ValidationErrors | null
{
  const value = control.value;
  const castedValue = +value;
  const numericalValue = Number.isNaN (castedValue) ? 0 : castedValue;
  if (numericalValue === 0)
    return {noPositiveValue: true};
  return null;
}

@Component ({
  selector: 'item-details',
  imports: [Suspense, ReactiveFormsModule, TextInput, Combobox, ListViewItemDetails, Button, NumberInput],
  templateUrl: './item-details.html',
  styleUrl: './item-details.scss',
  providers: [ItemsService]
})
export class ItemDetails extends ListViewDetailsFormLogicBase
{
  readonly itemsDetailsStore = inject (ItemsDetailsService);
  readonly itemsStore = inject (ItemsService);
  readonly categoriesStore = inject (CategoriesService);
  
  override form = new FormGroup ({
    name: new FormControl ("", [Validators.required]),
    description: new FormControl<string | null> (null),
    quantity: new FormControl (1, [Validators.required, positiveValueValidator]),
    unit: new FormControl ("unit", [Validators.required]),
    categories: new FormControl<string[]> ([], this.creating() ? [Validators.required] : undefined),
    components: new FormArray ([])
  });
  
  options = computed (
    () => this.categoriesStore.data()?.flatMap (MappingProfile.mapCategoryToOption) ?? []
  );
  
  items = computed (
    () =>
    {
      const apiData = this.itemsStore.data();
      return apiData?.flatMap (MappingProfile.mapItemToOption) ?? []
    }
  );
  
  readonly units = [
    {value: "unit", display: "Unit"},
    {value: "litre", display: "L"},
    {value: "kilogramme", display: "kg"},
  ]
  
  public get components ()
  {
    return this.form.get ("components") as FormArray;
  }
  
  public addExistingComponent ()
  {
    const component = new FormGroup ({
      id: new FormControl ("", [Validators.required]),
      quantity: new FormControl (1, [Validators.required, Validators.min (1)]),
    });
    this.components.push (component);
  }
  
  public addNewComponent ()
  {
    const component = new FormGroup ({
      name: new FormControl ("", [Validators.required]),
      description: new FormControl<string | null> (null),
      quantity: new FormControl (1, [Validators.required, Validators.min (1)])
    });
    this.components.push (component);
  }
  
  public removeComponent (index: number)
  {
    this.components.removeAt (index);
  }
  
  protected override executeOnInitIfCreating (): void
  {}
  
  protected override executeOnInitIfNotCreating (): void
  {
    this.itemsDetailsStore
        .setForm (this.form)
        .get ({path: this.id});
  }
  
  override ngOnInit () : void
  {
    super.ngOnInit();
    
    this.categoriesStore.get();
    
    if (!this.itemsStore.data())
      this.itemsStore.get ({path: "components"});
  }
  
  private arraysMatch<T> (apiDataArray?: T[], valueArray?: any) : boolean
  {
    if (!apiDataArray && !valueArray)
      return true;
    
    if (!apiDataArray && !!valueArray ||!!apiDataArray && !valueArray)
      return false;
      
    const strApiIds = apiDataArray!.map (MappingProfile.mapObjectToString);
    const strValuesIds = valueArray.map (MappingProfile.mapObjectToString);
    
    return strApiIds.length === strValuesIds.length
        && strApiIds.every (cId => strValuesIds.includes (cId))
  }
  
  protected override shouldMarkFormAsPristine (value: any): boolean
  {
    const apiData = this.itemsDetailsStore.data();
    
    if (this.creating())
      return value.name === ""
              && !value.description
              && value.quantity === 0
              && !value.unit
              && value.categories.length === 0
              && value.components.lenght === 0;
    else
      return value.name === apiData?.name
              && (!apiData?.description ? !value.description : value.description === apiData?.description)
              && value.quantity === apiData?.quantity
              && value.unit === apiData?.unit
              && this.arraysMatch (apiData?.categories, value?.categories)
              && this.arraysMatch (apiData?.components, value?.components);
  }
  
  public override onItemDelete (): void
  {
    this.itemsDetailsStore
        .setRouting (this.router, this.activatedRoute)
        .delete ({path: this.id});
  }
  
  public override handleFormSubmission (): void
  {
    if (this.creating())
    {
      this.itemsDetailsStore
          .setRouting (this.router, this.activatedRoute)
          .post ({body: this.form.value});
      return;
    }

    this.itemsDetailsStore.put ({path: this.id, body: this.form.value});
  }
}