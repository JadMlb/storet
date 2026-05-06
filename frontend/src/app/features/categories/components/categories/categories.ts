import { Component, inject } from '@angular/core';
import { Suspense } from '../../../../shared/components/suspense/suspense';
import { ListViewItem } from '../../../../shared/components/list-view-item/list-view-item';
import { CategoriesService } from '../../services/categories';
import { ListView } from '../../../../shared/components/list-view/list-view';
import { Button } from '../../../../shared/components/button/button';
import { ListViewLogicBase } from '../../../../shared/logic/ListViewLogicBase';

@Component ({
  selector: 'categories',
  imports: [ListViewItem, Suspense, Button, ListView],
  templateUrl: './categories.html',
  styleUrl: './categories.scss',
})
export class Categories extends ListViewLogicBase
{
  readonly categoriesStore = inject (CategoriesService);

  protected override onRefresh (): void
  {
    this.categoriesStore.get();
  }

  override ngOnInit (): void
  {
    this.categoriesStore.get();
    super.ngOnInit();
  }
}
