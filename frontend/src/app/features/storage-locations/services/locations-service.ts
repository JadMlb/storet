import { Injectable } from '@angular/core';
import { Service } from '../../../shared/services/service';
import { StorageLocation } from '../models/Location';

@Injectable ({
  providedIn: 'root',
})
export class LocationsService extends Service<StorageLocation[]>
{
  constructor ()
  {
    super ("storage");
  }
}