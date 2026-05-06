import { Component, inject } from '@angular/core';
import { ItemsService } from '../../services/items';
import { ListView } from '../../../../shared/components/list-view/list-view';
import { Button } from '../../../../shared/components/button/button';
import { Suspense } from '../../../../shared/components/suspense/suspense';
import { ListViewLogicBase } from '../../../../shared/logic/ListViewLogicBase';
import { ListViewItem } from '../../../../shared/components/list-view-item/list-view-item';
import { BehaviorSubject, switchMap } from 'rxjs';
import { ItemMetadataType } from '../../models/ItemType';
import { toSignal } from '@angular/core/rxjs-interop';

@Component ({
  selector: 'items',
  imports: [ListView, ListViewItem, Button, Suspense],
  templateUrl: './items.html',
  styleUrl: './items.scss',
})
export class Items extends ListViewLogicBase
{
  readonly itemsStore = inject (ItemsService);

  protected override onRefresh (): void
  {
    this.itemsStore.get();
  }

  override ngOnInit (): void
  {
    this.itemsStore.get();
    super.ngOnInit();
  }
}
