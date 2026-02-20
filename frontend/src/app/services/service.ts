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
	
	protected dataSignal = signal<TData[]> ([]);
	protected state = signal<ServiceStateType> ("idle");
	protected error = signal<ErrorType | null> (null);

	constructor (controllerName: string)
	{
		this.apiPath = `${environment.backendServerUrl}/api/${controllerName}`;
	}

	public getData ()
	{
		return this.dataSignal.asReadonly();
	}

	public data = this.dataSignal.asReadonly();
	
	public getState ()
	{
		return this.state.asReadonly();
	}
	
	public getError ()
	{
		return this.error.asReadonly();
	}

	public beforeRequest ()
	{
		this.state.set ("loading");
	}

	public handleSuccess (response: TData | TData[])
	{
		this.dataSignal.set (Array.isArray (response) ? response : [response]);
	}

	public afterSuccess (_: TData | TData[])
	{
		this.state.set ("idle");
	}

	public handleError (error: any)
	{
		this.error.set ({
			statusCode: 500,
			message: `${error}`
		});
	}

	public afterError (_: any)
	{
		this.state.set ("idle");
	}

	private appendToPath (path?: string)
	{
		if (!path)
			return this.apiPath;
		return `${this.apiPath}/${path}`;
	}

	public get (props?: {path?: string, params?: Record<string, string>})
	{
		const path = props?.path;
		const params = props?.params;
		
		this.beforeRequest();
		this.http.get<TData | TData[]> (this.appendToPath (path), {params})
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