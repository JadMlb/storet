import { Component, computed, DestroyRef, inject, input, output, signal } from '@angular/core';
import { Drawer } from '../drawer/drawer';
import { Button } from '../button/button';
import { NavigationService } from '../../services/layout/navigation-service';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { ConfirmDelete } from './confirm-delete/confirm-delete';
import { ResponsiveService } from '../../services/layout/responsive-service';
import { TooltipDirective } from '../../directives/tooltip/tooltip';

@Component ({
  selector: 'list-view-item-details',
  imports: [Drawer, Button, ConfirmDelete, TooltipDirective],
  templateUrl: './list-view-item-details.html',
  styleUrl: './list-view-item-details.scss',
})
export class ListViewItemDetails
{
  private readonly responsiveness = inject (ResponsiveService);
  private readonly navigation = inject (NavigationService);
  private readonly destroyRef = inject (DestroyRef);

  entity = input<string>();
  instanceIdentifier = input<string | null>();
  instanceLabel = input<string | null>();
  editing = input (false);
  creating = input (false);
  submitButtonDisabled = input (false);

  onEditingEnabled = output<void>();
  onDeleteItem = output<void>();
  onCancel = output<void>();
  onClose = output<void>();

  protected drawerOpen = signal (false);
  protected deleteDialogOpen = signal (false);

  protected instance = computed (
  	() =>
	{
		const id = this.instanceIdentifier();
		const entityName = this.entity();
		const entity = (entityName?.[0]?.toUpperCase() ?? "") + entityName?.slice (1);

		if (!id)
			return "";
		return `${entity} (${id})`;
	}
  );

  protected buttonsPaddingBlock = computed (
  	() => this.responsiveness.isSmallScreen() ? "medium" : "xsmall"
  );

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
