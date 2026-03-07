import { Component, inject } from '@angular/core';
import { ItemsService } from '../../services/items';
import { ListView } from '../../components/list-view/list-view';
import { Button } from '../../components/button/button';
import { Suspense } from '../../components/suspense/suspense';
import { ListViewLogicBase } from '../../common/ListViewLogicBase';
import { ListViewItem } from '../../components/list-view-item/list-view-item';

@Component ({
  selector: 'items',
  imports: [ListView, ListViewItem, Button, Suspense],
  templateUrl: './items.html',
  styleUrl: './items.scss',
})
export class Items extends ListViewLogicBase
{
  readonly itemsStore = inject (ItemsService);

  protected override onRefresh (): void
  {
    this.itemsStore.get();
  }

  override ngOnInit (): void
  {
    this.itemsStore.get();
    super.ngOnInit();
  }
}