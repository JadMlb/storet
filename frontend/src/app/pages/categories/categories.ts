import { Component, inject } from '@angular/core';
import { Suspense } from '../../components/suspense/suspense';
import { ListViewItem } from '../../components/list-view-item/list-view-item';
import { CategoriesService } from '../../services/categories';
import { ListView } from '../../components/list-view/list-view';
import { Button } from '../../components/button/button';
import { ListViewLogicBase } from '../../common/ListViewLogicBase';

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
