import { Component, computed, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Suspense } from '../../../../shared/components/suspense/suspense';
import { CategoryDetailsService } from '../../services/category-details';
import { TextInput } from '../../../../shared/components/text-input/text-input';
import { Combobox } from '../../../../shared/components/combobox/combobox';
import { CategoriesService } from '../../services/categories';
import { ListViewItemDetails } from '../../../../shared/components/list-view-item-details/list-view-item-details';
import { ListViewDetailsFormLogicBase } from '../../../../shared/logic/ListViewDetailsFormLogicBase';
import { CategoryMappingProfile } from '../../models/CategoryMappingProfile';

@Component ({
  selector: 'category-details',
  imports: [Suspense, ReactiveFormsModule, TextInput, Combobox, ListViewItemDetails],
  templateUrl: './category-details.html',
  styleUrl: './category-details.scss',
})
export class CategoryDetails extends ListViewDetailsFormLogicBase
{
  readonly categoriesDetailsStore = inject (CategoryDetailsService);
  readonly categoriesStore = inject (CategoriesService);
  
  override form = new FormGroup ({
    label: new FormControl ("", [Validators.required]),
    parentCategoryId: new FormControl<number | null> (null)
  });

  options = computed (
    () => (this.categoriesStore.data()?.flatMap (CategoryMappingProfile.mapCategoryToOption) ?? [])
            .filter (o => o.value !== this.id)
  );
  
  override executeOnInitIfNotCreating () : void
  {
    this.categoriesDetailsStore
        .setForm (this.form)
        .get ({path: this.id});
  }
  
  override executeOnInitIfCreating () : void
  {
  }
  
  override ngOnInit () : void
  {
    super.ngOnInit();
    
    if (this.categoriesStore.data())
      return;
    
    this.categoriesStore.get();
  }

  override handleFormSubmission (): void
  {
    if (this.creating())
    {
      this.categoriesDetailsStore
          .setRouting (this.router, this.activatedRoute)
          .post ({body: this.form.value});
      return;
    }

    this.categoriesDetailsStore.put ({path: this.id, body: this.form.value});
  }

  override onItemDelete (): void
  {
    this.categoriesDetailsStore
        .setRouting (this.router, this.activatedRoute)
        .delete ({path: this.id});
  }
  
  override shouldMarkFormAsPristine (value: any): boolean
  {
    const apiData = this.categoriesDetailsStore?.data();
    return (
      this.creating()
      && value.label === ""
      && value.parentCategoryId == null
    )
    ||
    (
      !this.creating()
      && value.label === apiData?.label
      && value.parentCategoryId == (apiData?.parentCategory?.id ?? null)
    )
  }
  
  protected override resetFormOnCancelEditing () : void
  {
    const apiData = this.categoriesDetailsStore.data();
    if (apiData)
      this.categoriesDetailsStore.updateForm (apiData);
    else
      this.form.reset();
  }
}