import { isApiError, isError, isValidationError } from "../types/Error";
import { ServiceEvent } from "../types/ServiceEvent";
import { ToastContentsInputType } from "../types/Toast";

export function toastFromEvent (event: ServiceEvent) : ToastContentsInputType | null
{
	if (event.type === "success")
		return {
			title: "Operation successful",
			type: "success"
		};
	const error = event.error;
	if (!error || !isError (error))
		return null;
	if (isValidationError (error))
		return {
			title: Object.values(error.errors).flatMap (messages => messages).join (", "),
			type: "failure"
		};
	if (isApiError (error))
		return {
			title: error.detail,
			type: "failure"
		};
	return null;
}