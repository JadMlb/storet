import { FormControl } from "@angular/forms";
import { CategoryMetadataType } from "./CategoryType";

export type Unit = "unit" | "kilogramme" | "litre";

export type ItemMetadataType = {
  id: string;
  name: string;
  description: string | null;
};

export type ItemType = ItemMetadataType & {
  quantity: number;
  unit: Unit;
  categories: number[];
  components: ItemCompositionType[];
};

export type ItemCompositionType = Partial<ItemMetadataType> & {
  quantity: number;
};

export type ExistingItemCompositionFormType = {
  id: FormControl<string>;
  quantity: FormControl<number>;
};

export type NewItemCompositionFormType = {
  name: FormControl<string>;
  description: FormControl<string | null>;
  quantity: FormControl<number>;
};