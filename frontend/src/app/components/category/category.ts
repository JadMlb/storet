import { Component, computed, EventEmitter, Input, Output, signal } from '@angular/core';
import { CategoryToggleExpandButton } from '../category-toggle-expand-button/category-toggle-expand-button';
import type { CategoryType } from "../../types/CategoryType";

@Component ({
  selector: 'category',
  imports: [CategoryToggleExpandButton],
  templateUrl: './category.html',
  styleUrl: './category.scss',
})
export class Category
{
  @Input() label!: string;
  @Input() children: CategoryType[] | undefined = [];
  @Input() depth: number = 0;
  
  @Output() onClick = new EventEmitter<MouseEvent>();
  
  isExpanded = signal (false);

  hasChildren = computed (
    () => (this.children?.length ?? 0) > 0
  );

  styleMargin = computed (
    () => ({
      marginLeft: `${this.depth * 20}px`
    })
  );

  handleToggleChange (value: boolean)
  {
    this.isExpanded.set (value);
  }

  propagateClick (e: MouseEvent)
  {
    this.onClick.emit (e);
  }
}
