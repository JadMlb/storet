import { Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { Drawer } from '../drawer/drawer';
import { Button } from '../button/button';
import { NavigationService } from '../../services/navigation-service';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { ConfirmDelete } from './confirm-delete/confirm-delete';

@Component ({
  selector: 'list-view-item-details',
  imports: [Drawer, Button, ConfirmDelete],
  templateUrl: './list-view-item-details.html',
  styleUrl: './list-view-item-details.scss',
})
export class ListViewItemDetails
{
  private readonly navigation = inject (NavigationService);
  private readonly destroyRef = inject (DestroyRef);

  entity = input<string>();
  editing = input (false);
  creating = input (false);
  submitButtonDisabled = input (false);

  onEditingEnabled = output<void>();
  onDeleteItem = output<void>();
  onCancel = output<void>();
  onClose = output<void>();

  protected drawerOpen = signal (false);
  protected deleteDialogOpen = signal (false);

  constructor ()
  {
    const id = toObservable (this.navigation.id);

    id.pipe (takeUntilDestroyed (this.destroyRef))
      .subscribe (
        value =>
        {
          this.drawerOpen.set (!!value);
        }
      );
  }
  
  handleCancel () : void
  {
    this.onCancel.emit();
  }
  
  handleDeleteItem () : void
  {
    this.deleteDialogOpen.set (true);
  }

  handleCancelDelete () : void
  {
  	this.deleteDialogOpen.set (false);
  }

  deleteItem () : void
  {
    this.onDeleteItem.emit();
    this.deleteDialogOpen.set (false);
  }
  
  handleEditingEnabled () : void
  {
    this.onEditingEnabled.emit();
  }
  
  handleClose () : void
  {
    this.onClose.emit();
  }
}
