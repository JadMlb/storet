import { Injectable } from '@angular/core';
import { ListViewItemType } from '../types/ListViewItem';
import { ActionType, Service } from './service';

@Injectable ({
  providedIn: 'root',
})
export class CategoriesService extends Service<ListViewItemType[]>
{
  constructor ()
  {
    super ("categories");
  }

  private static mapToCategoryType (data: any): ListViewItemType
  {
    return {
      id: data.id,
      label: data.label,
      children: data.subCategories?.map (CategoriesService.mapToCategoryType) ?? []
    };
  }

  override handleSuccess (actionType: ActionType, data: ListViewItemType[])
  {
    let categories = data.map (CategoriesService.mapToCategoryType);
    super.handleSuccess (actionType, categories);
  }
}
