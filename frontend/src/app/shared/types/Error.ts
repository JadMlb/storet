import { ToastContentsInputType } from "./Toast";

type ErrorResponseTypeBase = {
	title: string;
	status: number;
	traceId: string;
};

export type RequestValidationErrorResponseType = ErrorResponseTypeBase & {
	instance: string;
	errors: Record<string, string[]>
};

export type ApiErrorResponseType = ErrorResponseTypeBase & {
	type: string;
	detail: string;
};

export type ErrorResponseType = RequestValidationErrorResponseType | ApiErrorResponseType;

export function isError (obj: any) : obj is ErrorResponseType
{
	if (typeof (obj) !== "object" || !obj)
		return false;
	const objKeys = Object.keys (obj);
	return ["title", "status", "traceId"].every (key => objKeys.includes (key));
}

export function isValidationError (obj: any) : obj is RequestValidationErrorResponseType
{
	return isError (obj) && Object.keys(obj).includes ("errors");
}

export function isApiError (obj: any) : obj is ApiErrorResponseType
{
	return isError (obj) && Object.keys(obj).includes ("detail");
}