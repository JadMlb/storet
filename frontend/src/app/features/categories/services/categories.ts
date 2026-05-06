import { Injectable } from '@angular/core';
import { ListViewItemType } from '../../../shared/types/ListViewItem';
import { ActionType, Service } from '../../../shared/services/service';
import { ConvertableToOptionsArray } from '../../../shared/services/ConvertableToOptionsArray';
import { Option } from '../../../shared/types/Option';
import { CategoryMappingProfile } from '../models/CategoryMappingProfile';

@Injectable ({
  providedIn: 'root',
})
export class CategoriesService extends Service<ListViewItemType[]> implements ConvertableToOptionsArray
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

  public dataAsOptions (): Option[]
  {
    return this.data()?.flatMap (CategoryMappingProfile.mapCategoryToOption) ?? []
  }
}
