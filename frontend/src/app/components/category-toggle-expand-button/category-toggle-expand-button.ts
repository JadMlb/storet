import { Component, computed, EventEmitter, Input, Output, signal } from '@angular/core';

@Component ({
  selector: 'category-toggle-expand-button',
  imports: [],
  templateUrl: './category-toggle-expand-button.html',
  styleUrl: './category-toggle-expand-button.scss',
})
export class CategoryToggleExpandButton
{
  @Output() onToggle = new EventEmitter<boolean>();
  isExpanded = signal (false);
  
  style = computed (
    () => ({
      transform: `rotate(${this.isExpanded() ? 270 : 90}deg)`
    })
  );

  handleToggle (e: MouseEvent)
  {
    e.preventDefault();
    e.stopPropagation();
    this.isExpanded.set (!this.isExpanded());
    this.onToggle.emit (this.isExpanded());
  }
}
