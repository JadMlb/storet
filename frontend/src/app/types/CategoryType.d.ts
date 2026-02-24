export type CategoryType = {
	id: number;
	label: string;
	children?: CategoryType[];
};

export type CategoryWithParentType = {
	id: number;
	label: string;
	parentCategory?: CategoryType;
	children?: CategoryType[];
};

export type CategoryUpdateRequestType = {
	label?: string;
	parentCategoryId?: number;
};