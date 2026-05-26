export type ToastType = "info" | "warning" | "success" | "failure";

export type ToastContentsType = {
	id: string;
	type?: ToastType;
	title: string;
	description?: string;
	autoClears: boolean;
	clearing: boolean;
};

export type ToastContentsInputType = Omit<ToastContentsType, "id" | "autoClears" | "clearing"> & Partial<Pick<ToastContentsType, "autoClears">>;