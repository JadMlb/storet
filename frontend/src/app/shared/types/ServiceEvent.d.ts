import { ErrorResponseType } from "./Error";

export type SuccessServiceEvent = {
	type: "success";
};

export type ErrorServiceEvent = {
	type: "error";
	error: ErrorResponseType;
};

export type ServiceEvent = SuccessServiceEvent | ErrorServiceEvent;