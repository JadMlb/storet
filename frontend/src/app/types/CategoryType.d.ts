export type CategoryMetadataType = {
	id: number;
	label: string;
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