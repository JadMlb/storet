import { Injectable } from '@angular/core';
import { CategoryWithParentType } from '../models/CategoryType';
import { DetailsService } from '../../../shared/services/details-service';

@Injectable ({
  providedIn: 'root',
})
export class CategoryDetailsService extends DetailsService<CategoryWithParentType>
{
  constructor ()
  {
    super ("categories");
  }

  public override updateForm (response: any)
  {
    const mappedResponse = {label: response.label, parentCategoryId: response.parentCategory?.id ?? null};
    
    this.form!.patchValue (mappedResponse);
  }
}
