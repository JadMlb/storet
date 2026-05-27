export type PathProps = {
	path?: string | null;
};

export type GetPathProps = PathProps & {
	params?: Record<string, string>;
};

export type PostPathProps = PathProps & {
	body: any;
};