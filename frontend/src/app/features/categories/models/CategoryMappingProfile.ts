import { MappingProfile } from "../../../shared/logic/MappingProfile";
import { ListViewItemType } from "../../../shared/types/ListViewItem";
import { Option } from "../../../shared/types/Option";

export abstract class CategoryMappingProfile extends MappingProfile
{
  public static mapCategoryToOption (category: ListViewItemType): Option[]
  {
    return [
      {value: `${category.id}`, display: category.label},
      ...category.children?.flatMap (CategoryMappingProfile.mapCategoryToOption) ?? []
    ];
  }
}