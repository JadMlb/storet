import { Component, computed, inject } from '@angular/core';
import { Suspense } from '../../components/suspense/suspense';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TextInput } from '../../components/input/input';
import { Combobox } from '../../components/combobox/combobox';
import { ListViewItemDetails } from '../../components/list-view-item-details/list-view-item-details';
import { ListViewDetailsFormLogicBase } from '../../common/ListViewDetailsFormLogicBase';
import { ItemsDetailsService } from '../../services/items-details';
import { CategoriesService } from '../../services/categories';
import { MappingProfile } from '../../common/MappingProfile';
import { ItemType } from '../../types/ItemType';

@Component ({
  selector: 'item-details',
  imports: [Suspense, ReactiveFormsModule, TextInput, Combobox, ListViewItemDetails],
  templateUrl: './item-details.html',
  styleUrl: './item-details.scss',
})
export class ItemDetails extends ListViewDetailsFormLogicBase
{
  readonly itemsDetailsStore = inject (ItemsDetailsService);
  readonly categoriesStore = inject (CategoriesService);
  
  override form = new FormGroup ({
    name: new FormControl ("", [Validators.required]),
    description: new FormControl<string | null> (null),
    categories: new FormControl<string[]> ([], this.creating() ? [Validators.required] : undefined)
  });
  
  options = computed (
    () => this.categoriesStore.data()?.flatMap (MappingProfile.mapCategoryToOption) ?? []
  );
  
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
  }
  
  private categoriesArraysMatch (apiData: ItemType | null, value: any) : boolean
  {
    if (!apiData?.categories && !value?.categories)
      return true;
    
    if (!apiData?.categories && !!value?.categories ||!!apiData?.categories && !value?.categories)
      return false;
      
    const strApiCategoryIds = apiData!.categories.map (c => `${c}`);
    const strValuesCategoryIds = value.categories.map ((c: any) => `${c}`);
    
    return strApiCategoryIds.length === strValuesCategoryIds.length
        && strApiCategoryIds.every (cId => strValuesCategoryIds.includes (cId))
  }
  
  protected override shouldMarkFormAsPristine (value: any): boolean
  {
    const apiData = this.itemsDetailsStore.data();
    
    if (this.creating())
      return value.name === ""
              && !value.description
              && value.categories.length === 0;
    else
      return value.name === apiData?.name
              && (!apiData?.description ? !value.description : value.description === apiData?.description)
              && this.categoriesArraysMatch (apiData, value);
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