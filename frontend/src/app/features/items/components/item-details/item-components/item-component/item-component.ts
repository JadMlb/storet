import { Component, computed, inject, input, OnInit, output } from '@angular/core';
import { AbstractControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Button } from '../../../../../../shared/components/button/button';
import { Combobox } from '../../../../../../shared/components/combobox/combobox';
import { TextInput } from '../../../../../../shared/components/text-input/text-input';
import { NumberInput } from '../../../../../../shared/components/number-input/number-input';
import { ItemsService } from '../../../../services/items';
import { ItemMappingProfile } from '../../../../models/ItemMappingProfile';

@Component ({
  selector: 'item-component',
  imports: [Button, Combobox, TextInput, NumberInput, ReactiveFormsModule],
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

  items = computed (
    () =>
    {
      const apiData = this.itemsStore.data();
      return apiData?.flatMap (ItemMappingProfile.mapItemToOption) ?? []
    }
  );

  handleItemDelete ()
  {
    this.onItemDelete.emit (this.index());
  }

  ngOnInit () : void
  {
    console.log ("item form receieved", this.component()?.value)
  }
}