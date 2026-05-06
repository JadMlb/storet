import { Component, computed, EventEmitter, Output, signal } from '@angular/core';
import { Chevron } from '../chevron/chevron';

@Component ({
  selector: 'list-view-item-toggle-expand-button',
  imports: [Chevron],
  templateUrl: './list-view-item-toggle-expand-button.html',
  styleUrl: './list-view-item-toggle-expand-button.scss',
})
export class ListViewItemToggleExpandButton
{
  @Output() onToggle = new EventEmitter<boolean>();
  isExpanded = signal (false);
  
  chevronRotation = computed (
    () => this.isExpanded() ? "top" : "bottom"
  );

  handleToggle (e: MouseEvent)
  {
    e.preventDefault();
    e.stopPropagation();
    this.isExpanded.set (!this.isExpanded());
    this.onToggle.emit (this.isExpanded());
  }
}
