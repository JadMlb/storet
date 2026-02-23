import { Component, computed, EventEmitter, Input, Output, signal } from '@angular/core';
import { Chevron } from '../chevron/chevron';

@Component ({
  selector: 'category-toggle-expand-button',
  imports: [Chevron],
  templateUrl: './category-toggle-expand-button.html',
  styleUrl: './category-toggle-expand-button.scss',
})
export class CategoryToggleExpandButton
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
