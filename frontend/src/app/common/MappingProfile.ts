import { ItemMetadataType } from '../types/ItemType';
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
  
  public static mapItemToOption (item: ItemMetadataType): Option[]
  {
    return [
      {value: `${item.id}`, display: item.name},
    ];
  }
  
  public static mapObjectToString (value: any) : string
  {
    if (!value)
      return "";
      
    if (typeof value === "object")
      return JSON.stringify (
                    Object.fromEntries (
                      Object.entries (value)
                            .sort ((e1, e2) => e1[0] > e2[0] ? 1 : -1)
                    )
                  );
      
    return `${value}`;
  }
}