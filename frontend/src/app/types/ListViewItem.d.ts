export type ListViewItemType = {
  id: string;
  label: string;
  children?: ListViewItemType[];
};