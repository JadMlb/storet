import { ListViewItemType } from '../types/ListViewItem';
import { Option } from "../types/Option";

export class MappingProfile
{
  public static mapCategoryToOption (category: ListViewItemType): Option[]
  {
    return [
      {value: `${category.id}`, display: category.label},
      ...category.children?.flatMap (MappingProfile.mapCategoryToOption) ?? []
    ];
  }
}