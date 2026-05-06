import { Component, computed, inject } from '@angular/core';
import { Suspense } from '../../../../shared/components/suspense/suspense';
import { AbstractControl, FormArray, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { TextInput } from '../../../../shared/components/text-input/text-input';
import { Combobox } from '../../../../shared/components/combobox/combobox';
import { ListViewItemDetails } from '../../../../shared/components/list-view-item-details/list-view-item-details';
import { ListViewDetailsFormLogicBase } from '../../../../shared/logic/ListViewDetailsFormLogicBase';
import { ItemsDetailsService } from '../../services/items-details';
import { CategoriesService } from '../../../categories/services/categories';
import { ItemMappingProfile } from '../../models/ItemMappingProfile';
import { Button } from '../../../../shared/components/button/button';
import { NumberInput } from '../../../../shared/components/number-input/number-input';
import { ItemsService } from '../../services/items';
import { NoEditWarning } from './no-edit-warning/no-edit-warning';
import { ItemComponents } from './item-components/item-components';

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
  imports: [Suspense, ReactiveFormsModule, TextInput, Combobox, ListViewItemDetails, NumberInput, NoEditWarning, ItemComponents],
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
  
  readonly ONLY_ALLOW_EDIT_ON_INSERT_FIELDS: (keyof typeof this.form.controls)[] = ["quantity", "unit", "components"];

  options = computed (
    () => this.categoriesStore.dataAsOptions()
  );
  
  items = computed (
    () =>
    {
      const apiData = this.itemsStore.data();
      return apiData?.flatMap (ItemMappingProfile.mapItemToOption) ?? []
    }
  );

  // shouldDisplayTotalStock = computed (
  //   () => this.form.controls.components.length === 0 && this.itemsDetailsStore.data()?.stock
  // );

  // totalStock = this.itemsDetailsStore.data()?.stock;
  
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
    this.components.markAsDirty();
  }
  
  protected override executeOnInitIfCreating (): void
  {}
  
  protected override executeOnInitIfNotCreating (): void
  {
    this.itemsDetailsStore
        .get ({path: this.id});
  }
  
  override ngOnInit () : void
  {
    this.itemsDetailsStore.setForm(this.form).setRouting (this.router, this.activatedRoute);

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
      
    const strApiIds = apiDataArray!.map (ItemMappingProfile.mapObjectToString);
    const strValuesIds = valueArray.map (ItemMappingProfile.mapObjectToString);
    
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
              && this.arraysMatch (apiData?.categories, value?.categories);
  }
  
  public override onEditingEnabled () : void
  {
    super.onEditingEnabled();
    
    this.ONLY_ALLOW_EDIT_ON_INSERT_FIELDS.forEach (
      field => this.form.controls[field].disable()
    );
  }
  
  protected override resetFormOnCancelEditing () : void
  {
    const apiData = this.itemsDetailsStore.data();
    if (apiData)
      this.itemsDetailsStore.updateForm (apiData);
    else
      this.form.reset();
  }
  
  public override onItemDelete (): void
  {
    this.itemsDetailsStore
        .delete ({path: this.id});
  }
  
  public override handleFormSubmission (): void
  {
    if (this.creating())
    {
      this.itemsDetailsStore
          .post ({body: this.form.value});
      return;
    }

    this.itemsDetailsStore.put ({path: this.id, body: this.form.value});
  }
}