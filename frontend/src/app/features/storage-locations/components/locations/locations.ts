import { Component, inject } from '@angular/core';
import { ListViewLogicBase } from '../../../../shared/logic/ListViewLogicBase';
import { LocationsService } from '../../services/locations-service';
import { ListViewItem } from '../../../../shared/components/list-view-item/list-view-item';
import { ListView } from '../../../../shared/components/list-view/list-view';
import { Suspense } from '../../../../shared/components/suspense/suspense';
import { Button } from '../../../../shared/components/button/button';
import { LocationDetails } from '../location-details/location-details';

@Component ({
  selector: 'locations',
  imports: [ListView, ListViewItem, Suspense, Button, LocationDetails],
  templateUrl: './locations.html',
  styleUrl: './locations.scss',
})
export class Locations extends ListViewLogicBase
{
  readonly locationsStore = inject (LocationsService);

  protected override onRefresh (): void
  {
    this.locationsStore.get();
  }

  override ngOnInit (): void
  {
    this.locationsStore.get();
    super.ngOnInit();
  }
}