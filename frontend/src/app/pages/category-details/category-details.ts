import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Drawer } from '../../components/drawer/drawer';
import { Suspense } from '../../components/suspense/suspense';
import { CategoryDetailsService } from '../../services/category-details';
import { TextInput } from '../../components/input/input';
import { Combobox } from '../../components/combobox/combobox';
import { CategoriesService } from '../../services/categories';
import { Option } from '../../types/Option';
import { CategoryType } from '../../types/CategoryType';
import { Button } from '../../components//button/button';

@Component ({
  selector: 'category-details',
  imports: [Drawer, Suspense, ReactiveFormsModule, TextInput, Combobox, Button],
  templateUrl: './category-details.html',
  styleUrl: './category-details.scss',
})
export class CategoryDetails implements OnInit
{
  private readonly activatedRoute = inject (ActivatedRoute);
  private readonly router = inject (Router);
  readonly categoriesDetailsStore = inject (CategoryDetailsService);
  readonly categoriesStore = inject (CategoriesService);
  
  creating = signal (false);
  editing = signal (false);
  private editsHappened = false;

  form = new FormGroup ({
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

  private id = this.activatedRoute.snapshot.paramMap.get ("id");

  options = computed (
    () => (this.categoriesStore.data()?.flatMap (CategoryDetails.mapCategoryToOption) ?? [])
            .filter (o => o.value !== this.id)
  );

  ngOnInit ()
  {
    this.creating.set (this.activatedRoute.snapshot.url[0].path === "new");
    if (this.creating())
    {
      this.editing.set (true);
    }
    else
    {
      this.categoriesDetailsStore
          .setForm (this.form)
          .get ({path: this.id});
      this.categoriesStore.get();
      this.form.disable();
    }
    this.subscribeToFormChanges();
  }

  navBack (): void
  {
    this.router.navigate (
      ["../"],
      {
        relativeTo: this.activatedRoute,
        state: {refresh: this.editsHappened, timestamp: Date.now()}
      }
    );
  }

  onSubmit (): void
  {
    if (!this.form.valid)
      return;

    if (this.creating())
    {
      this.categoriesDetailsStore
          .setRouting (this.router, this.activatedRoute)
          .post ({body: this.form.value});
      return;
    }

    this.categoriesDetailsStore.put ({path: this.id, body: this.form.value});
  }

  deleteCategory (): void
  {
    this.categoriesDetailsStore
        .setRouting (this.router, this.activatedRoute)
        .delete ({path: this.id});
  }

  private subscribeToFormChanges (): void
  {
    this.form.valueChanges
              .subscribe (
                value =>
                {
                  const apiData = this.categoriesDetailsStore.data();
                  if (
                    (
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
                  )
                  {
                    this.form.markAsPristine();
                    this.editsHappened = false;
                  }
                  else
                    this.editsHappened = true;
                }
              );
  }

  enableEditing ()
  {
    this.form.enable();
    this.editing.set (true);
  }
  
  onCancel () : void
  {
    if (this.creating())
    {
      this.navBack();
      return;
    }

    this.form.disable();
    this.editing.set (false);
  }
}