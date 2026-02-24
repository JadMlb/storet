import { Injectable } from '@angular/core';
import { CategoryType } from '../types/CategoryType';
import { ActionType, Service } from './service';

@Injectable ({
  providedIn: 'root',
})
export class CategoriesService extends Service<CategoryType[]>
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

  override handleSuccess (actionType: ActionType, data: CategoryType[])
  {
    let categories = data.map (CategoriesService.mapToCategoryType);
    super.handleSuccess (actionType, categories);
  }
}
