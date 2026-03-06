import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Suspense } from '../../components/suspense/suspense';
import { CategoryDetailsService } from '../../services/category-details';
import { TextInput } from '../../components/input/input';
import { Combobox } from '../../components/combobox/combobox';
import { CategoriesService } from '../../services/categories';
import { Option } from '../../types/Option';
import { CategoryType } from '../../types/CategoryType';
import { ListViewItemDetails } from '../../components/list-view-item-details/list-view-item-details';
import { ListViewDetailsFormLogicBase } from '../../common/ListViewDetailsFormLogicBase';

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

  private static mapCategoryToOption (category: CategoryType): Option[]
  {
    return [
      {value: `${category.id}`, display: category.label},
      ...category.children?.flatMap (CategoryDetails.mapCategoryToOption) ?? []
    ];
  }

  options = computed (
    () => (this.categoriesStore.data()?.flatMap (CategoryDetails.mapCategoryToOption) ?? [])
            .filter (o => o.value !== this.id)
  );
  
  override executeOnInitIfNotCreating () : void
  {
    this.categoriesDetailsStore
        .setForm (this.form)
        .get ({path: this.id});
    this.categoriesStore.get();
  }

  override onFormSubmit (): void
  {
    super.onFormSubmit();
    
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
}