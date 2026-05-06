import { Component, computed, inject, input, OnInit, output } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Button } from '../../../../../../shared/components/button/button';
import { Combobox } from '../../../../../../shared/components/combobox/combobox';
import { TextInput } from '../../../../../../shared/components/text-input/text-input';
import { NumberInput } from '../../../../../../shared/components/number-input/number-input';
import { ItemsService } from '../../../../services/items';
import { ItemMappingProfile } from '../../../../models/ItemMappingProfile';
import { StockInfo } from '../../../../../inventory/components/stock-info/stock-info';
import { InventoryDetailsService } from '../../../../../inventory/services/inventory-details-service';

@Component ({
  selector: 'item-component',
  imports: [Button, Combobox, TextInput, NumberInput, ReactiveFormsModule, StockInfo],
  templateUrl: './item-component.html',
  styleUrl: './item-component.scss',
})
export class ItemComponent implements OnInit
{
  index = input.required<number>();
  component = input.required<FormGroup>();
  creating = input<boolean> (false);

  onItemDelete = output<number>();

  readonly itemsStore = inject (ItemsService);
  readonly inventoryDetailsStore = inject (InventoryDetailsService);

  items = computed (
    () =>
    {
      const apiData = this.itemsStore.data();
      return apiData?.flatMap (ItemMappingProfile.mapItemToOption) ?? []
    }
  );

  public get stock ()
  {
    return this.inventoryDetailsStore.data()?.[0];
  }

  handleItemDelete ()
  {
    this.onItemDelete.emit (this.index());
  }

  ngOnInit () : void
  {
    const componentId = this.component().get("id")?.value;
    console.log (componentId);
    if (!componentId)
      return;

    if (this.inventoryDetailsStore.data()?.[0]?.itemId !== componentId)
      this.inventoryDetailsStore.get ({path: componentId});
  }
}