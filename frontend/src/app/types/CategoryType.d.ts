export type CategoryType = {
	id: number;
	label: string;
	children?: CategoryType[];
};