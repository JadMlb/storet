import { Injectable } from '@angular/core';
import { CategoryWithParentType } from '../types/CategoryType';
import { DetailsService } from './details-service';

@Injectable ({
  providedIn: 'root',
})
export class CategoryDetailsService extends DetailsService<CategoryWithParentType>
{
  constructor ()
  {
    super ("categories");
  }

  protected override mapResponseToFormData (response: any)
  {
    return {label: response.label, parentCategoryId: response.parentCategory?.id ?? null};
  }
}
