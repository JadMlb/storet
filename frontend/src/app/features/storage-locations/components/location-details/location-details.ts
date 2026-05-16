import { Component, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ListViewDetailsFormLogicBase } from '../../../../shared/logic/ListViewDetailsFormLogicBase';
import { LocationDetailsService } from '../../services/location-details-service';
import { ListViewItemDetails } from '../../../../shared/components/list-view-item-details/list-view-item-details';
import { Suspense } from '../../../../shared/components/suspense/suspense';
import { TextInput } from '../../../../shared/components/text-input/text-input';

@Component ({
  selector: 'location-details',
  imports: [ListViewItemDetails, ReactiveFormsModule, Suspense, TextInput],
  templateUrl: './location-details.html',
  styleUrl: './location-details.scss',
})
export class LocationDetails extends ListViewDetailsFormLogicBase
{
  protected readonly locationDetailsStore = inject (LocationDetailsService);

  override form = new FormGroup ({
    name: new FormControl ("", [Validators.required]),
    description: new FormControl<string | null> (null)
  });

  override initialiseData (creating: boolean = false) : void
  {
    super.initialiseData (creating);

    this.locationDetailsStore
        .setForm (this.form)
        .setRouting (this.navigation);
  }

  protected override executeOnInitIfNotCreating () : void
  {
    this.locationDetailsStore
        .get ({path: this.navigation.id()});
  }

  protected override executeOnInitIfCreating () : void
  {}
  
  protected override shouldMarkFormAsPristine (value: any) : boolean
  {
    const apiData = this.locationDetailsStore.data();

    if (this.creating())
      return value.name === ""
              && !value.description;
    return value.name === apiData?.name
              && (!apiData?.description ? !value.description : value.description === apiData.description);
  }
  
  protected override resetFormOnCancelEditing () : void
  {
    const apiData = this.locationDetailsStore.data();
    if (apiData)
      this.locationDetailsStore.updateForm (apiData);
    else
      this.form.reset();
  }
  
  protected override handleFormSubmission () : void
  {
    if (this.creating())
    {
      this.locationDetailsStore
        .post ({body: this.form.value});
      return;
    }

    this.locationDetailsStore.put ({path: this.navigation.id(), body: this.form.value});
  }
  
  public override onItemDelete () : void
  {
    this.locationDetailsStore
        .delete ({path: this.navigation.id()});
  }
}