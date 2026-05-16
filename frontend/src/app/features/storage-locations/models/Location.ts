export type StorageLocation = {
  id: string;
  name: string;
};

export type StorageLocationDetails = StorageLocation & {
  description?: string;
};