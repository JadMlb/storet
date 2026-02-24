import { HttpClient } from "@angular/common/http";
import { inject, signal } from "@angular/core";
import { environment } from "../environments/environment";

export type ServiceStateType = "loading" | "idle";
export type ErrorType = {
	id?: string;
	statusCode: number;
	message: string;
};

type PathProps = {
	path?: string | null;
};

type GetPathProps = PathProps & {
	params?: Record<string, string>;
};

type PostPathProps = PathProps & {
	body: any;
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

	public handleSuccess (response: TData | null)
	{
		this.dataSignal.set (response);
	}

	public afterSuccess (_: TData | null)
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

	public get (props?: GetPathProps)
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

	public post ({path, body}: PostPathProps)
	{
		this.beforeRequest();
		this.http.post<TData> (this.appendToPath (path), body)
					.subscribe ({
						next: response =>
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
	
	public put ({path, body}: PostPathProps)
	{
		this.beforeRequest();
		this.http.put<TData> (this.appendToPath (path), body)
					.subscribe ({
						next: response =>
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

	public delete ({path}: PathProps)
	{
		this.beforeRequest();
		this.http.delete (this.appendToPath (path))
					.subscribe ({
						next: () =>
						{
							this.handleSuccess (null);
							this.afterSuccess (null);
						},
						error: (error) =>
						{
							this.handleError (error);
							this.afterError (error);
						}
					});
	}
}