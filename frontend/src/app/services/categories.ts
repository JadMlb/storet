import { Injectable } from '@angular/core';
import { CategoryType } from '../types/CategoryType';
import { Service } from './service';

@Injectable ({
  providedIn: 'root',
})
export class CategoriesService extends Service<CategoryType>
{
  constructor ()
  {
    super ("categories");
  }

  private static mapToCategoryType (data: any): CategoryType
  {
    return {
      id: data.id,
      label: data.label,
      children: data.subCategories?.map (CategoriesService.mapToCategoryType) ?? []
    };
  }

  override handleSuccess (data: CategoryType | CategoryType[])
  {
    let categories;
    if (Array.isArray (data))
      categories = data.map (CategoriesService.mapToCategoryType);
    else
      categories = [CategoriesService.mapToCategoryType (data)];
    super.handleSuccess (categories);
  }
}
