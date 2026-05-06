import { booleanAttribute, Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Drawer } from '../drawer/drawer';
import { Button } from '../button/button';

@Component ({
  selector: 'list-view-item-details',
  imports: [Drawer, Button],
  templateUrl: './list-view-item-details.html',
  styleUrl: './list-view-item-details.scss',
})
export class ListViewItemDetails
{
  protected readonly activatedRoute = inject (ActivatedRoute);
  protected readonly router = inject (Router);
  protected readonly id = this.activatedRoute.snapshot.paramMap.get ("id");
  
  @Input ({transform: booleanAttribute}) editing: boolean = false;
  @Input ({transform: booleanAttribute}) creating: boolean = false;
  @Input ({transform: booleanAttribute}) submitButtonDisabled: boolean = true;

  @Output() onEditingEnabled = new EventEmitter<void>();
  @Output() onDeleteItem = new EventEmitter<void>();
  @Output() onCancel = new EventEmitter<void>();
  @Output() onClose = new EventEmitter<void>();
  
  handleCancel () : void
  {
    this.onCancel.emit();
  }
  
  handleDeleteItem () : void
  {
    this.onDeleteItem.emit();
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
