import { CategoryMetadataType } from "./CategoryType";

export type ItemMetadataType = {
  id: string;
  name: string;
  description: string | null;
};

export type ItemType = ItemMetadataType & {
  categories: number[]
};