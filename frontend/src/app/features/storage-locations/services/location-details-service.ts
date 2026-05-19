import { Injectable } from '@angular/core';
import { DetailsService } from '../../../shared/services/data/details-service';
import { StorageLocationDetails } from '../models/Location';

@Injectable ({
  providedIn: 'root',
})
export class LocationDetailsService extends DetailsService<StorageLocationDetails>
{
  constructor ()
  {
    super ("storage");
  }

  public override updateForm (response: any) : void
  {
    this.form?.patchValue ({name: response.name, description: response.description});
  }
}
