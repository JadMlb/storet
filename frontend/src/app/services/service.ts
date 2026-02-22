import { HttpClient } from "@angular/common/http";
import { inject, signal } from "@angular/core";
import { environment } from "../environments/environment";

export type ServiceStateType = "loading" | "idle";
export type ErrorType = {
	id?: string;
	statusCode: number;
	message: string;
};

export abstract class Service<TData>
{
	protected http = inject (HttpClient);
	protected apiPath: string;
	
	protected dataSignal = signal<TData | null> (null);
	protected stateSignal = signal<ServiceStateType> ("idle");
	protected errorSignal = signal<ErrorType | null> (null);

	constructor (controllerName: string)
	{
		this.apiPath = `${environment.backendServerUrl}/api/${controllerName}`;
	}

	public readonly data = this.dataSignal.asReadonly();
	public readonly state = this.stateSignal.asReadonly();
	public readonly error = this.errorSignal.asReadonly();

	public beforeRequest ()
	{
		this.stateSignal.set ("loading");
	}

	public handleSuccess (response: TData)
	{
		this.dataSignal.set (response);
	}

	public afterSuccess (_: TData)
	{
		this.stateSignal.set ("idle");
	}

	public handleError (error: any)
	{
		this.errorSignal.set ({
			statusCode: 500,
			message: `${error}`
		});
	}

	public afterError (_: any)
	{
		this.stateSignal.set ("idle");
	}

	private appendToPath (path?: string | null)
	{
		if (!path)
			return this.apiPath;
		return `${this.apiPath}/${path}`;
	}

	public get (props?: {path?: string | null, params?: Record<string, string>})
	{
		const path = props?.path;
		const params = props?.params;
		
		this.beforeRequest();
		this.http.get<TData> (this.appendToPath (path), {params})
					.subscribe ({
						next: (response) =>
						{
							this.handleSuccess (response);
							this.afterSuccess (response);
						},
						error: (error) =>
						{
							this.handleError (error);
							this.afterError (error);
						}
					});
	}
}