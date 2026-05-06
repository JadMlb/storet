import { Component, input, output } from '@angular/core';
import { FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ItemComponent } from './item-component/item-component';
import { Button } from '../../../../../shared/components/button/button';
import { NoEditWarning } from '../no-edit-warning/no-edit-warning';
import { FieldLabel } from '../../../../../shared/components/field-label/field-label';

@Component ({
  selector: 'item-components',
  imports: [ReactiveFormsModule, ItemComponent, Button, NoEditWarning, FieldLabel],
  templateUrl: './item-components.html',
  styleUrl: './item-components.scss',
})
export class ItemComponents
{
  components = input<FormArray<FormGroup>> (new FormArray<any> ([]));
  creating = input<boolean> (false);

  onAddExisting = output<void>();
  onAddNew = output<void>();
  onRemoveItem = output<number>();

  public addExistingComponent ()
  {
    this.onAddExisting.emit();
  }
  
  public addNewComponent ()
  {
    this.onAddNew.emit();
  }

  public removeItem (index: number)
  {
    this.onRemoveItem.emit (index);
  }
}
