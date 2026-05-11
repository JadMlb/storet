import { Component, inject, input, OnInit, output, signal } from '@angular/core';
import { StockWithBounds, Stock, StockBounds } from '../../models/Stock';
import { StockTag } from './stock-tag/stock-tag';
import { Button } from '../../../../shared/components/button/button';
import { InventoryBoundsEditor } from './inventory-bounds-editor/inventory-bounds-editor';
import { InventoryDetailsService } from '../../services/inventory-details-service';

@Component ({
  selector: 'stock-info',
  imports: [StockTag, Button, InventoryBoundsEditor],
  templateUrl: './stock-info.html',
  styleUrl: './stock-info.scss',
})
export class StockInfo implements OnInit
{
  prefetchedInfo = input<Stock | null>();
  itemId = input.required<string>();
  itemName = input<string | null>();
  disableEditing = input<boolean> (false);

  editDialogOpen = signal (false);

  private readonly inventoryDetailsStore = inject (InventoryDetailsService);

  public ngOnInit () : void
  {
    if (this.prefetchedInfo())
      return;

    if (this.stock?.itemId !== this.itemId())
      this.inventoryDetailsStore.get ({path: this.itemId()});
  }

  protected get stock () : StockWithBounds | null
  {
    return (this.prefetchedInfo() as StockWithBounds) ??
            this.inventoryDetailsStore.data()?.[0] ?? null;
  }

  public openEditDialog (e: Event) : void
  {
    e.stopPropagation();
    e.preventDefault();

    if (this.disableEditing() || !this.stock)
      return;

    this.editDialogOpen.set (true);
  }

  public closeDialog () : void
  {
    this.editDialogOpen.set (false);
  }

  public saveData (data: StockBounds) : void
  {
    this.inventoryDetailsStore.put ({
      path: this.itemId(),
      body: data
    });
  }
}