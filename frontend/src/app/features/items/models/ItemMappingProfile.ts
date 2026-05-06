import { MappingProfile } from "../../../shared/logic/MappingProfile";
import { Option } from "../../../shared/types/Option";
import { ItemMetadataType } from "./ItemType";

export abstract class ItemMappingProfile extends MappingProfile
{
  public static mapItemToOption (item: ItemMetadataType): Option[]
  {
    return [
      {value: `${item.id}`, display: item.name},
    ];
  }
}