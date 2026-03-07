import { Component, computed, EventEmitter, Input, numberAttribute, Output, signal } from '@angular/core';
import { ListViewItemToggleExpandButton } from '../list-view-item-toggle-expand-button/list-view-item-toggle-expand-button';
import { ListViewItemType } from '../../types/ListViewItem';

@Component ({
  selector: 'list-view-item',
  imports: [ListViewItemToggleExpandButton],
  templateUrl: './list-view-item.html',
  styleUrl: './list-view-item.scss',
})
export class ListViewItem
{
  @Input() id!: string;
  @Input() label!: string;
  @Input() children?: ListViewItemType[] = [];
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
