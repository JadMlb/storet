import { Component, inject } from '@angular/core';
import { ItemsService } from '../../services/items';
import { ListView } from '../../../../shared/components/list-view/list-view';
import { Button } from '../../../../shared/components/button/button';
import { Suspense } from '../../../../shared/components/suspense/suspense';
import { ListViewLogicBase } from '../../../../shared/logic/ListViewLogicBase';
import { ListViewItem } from '../../../../shared/components/list-view-item/list-view-item';
import { StockInfo } from '../../../inventory/components/stock-info/stock-info';
import { InventoryService } from '../../../inventory/services/inventory-service';
import { switchMap } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { ItemStock } from '../../../inventory/models/Stock';

@Component ({
  selector: 'items',
  imports: [ListView, ListViewItem, Button, Suspense, StockInfo],
  templateUrl: './items.html',
  styleUrl: './items.scss',
})
export class Items extends ListViewLogicBase
{
  readonly itemsStore = inject (ItemsService);
  readonly stockStore = inject (InventoryService);

  private readonly stock$ = this.itemsStore.data$.pipe (
    switchMap (
      items =>
      {
        this.stockStore.getInventories (items?.map (i => i.id) ?? []);
        return this.stockStore.data$;
      }
    )
  );

  stock = toSignal (this.stock$, {} as ItemStock);

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
