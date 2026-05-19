import { HttpClient } from "@angular/common/http";
import { inject, signal } from "@angular/core";
import { environment } from "../../../../environments/environment";
import { Option } from "../../types/Option";
import { BehaviorSubject } from "rxjs";
import { toObservable } from "@angular/core/rxjs-interop";

export type ServiceStateType = "loading" | "idle";
export type ErrorType = {
	id?: string;
	statusCode: number;
	message: string;
};

export type PathProps = {
	path?: string | null;
};

export type GetPathProps = PathProps & {
	params?: Record<string, string>;
};

export type PostPathProps = PathProps & {
	body: any;
};

export type ActionType = "GET" | "POST" | "PUT" | "DELETE";

export abstract class Service<TData>
{
	protected http = inject (HttpClient);
	protected apiBasePath: string;
	protected apiPath: string;
	
	protected dataSignal = signal<TData | null> (null);
	protected stateSignal = signal<ServiceStateType> ("idle");
	protected errorSignal = signal<ErrorType | null> (null);

	constructor (controllerName: string)
	{
		this.apiBasePath = `${environment.backendServerUrl}/api`;
		this.apiPath = `${this.apiBasePath}/${controllerName}`;
	}

	public readonly data = this.dataSignal.asReadonly();
	public readonly state = this.stateSignal.asReadonly();
	public readonly error = this.errorSignal.asReadonly();
	public readonly data$ = toObservable (this.dataSignal);

	public beforeRequest ()
	{
		this.stateSignal.set ("loading");
	}

	public handleSuccess (_: ActionType, response: TData | null)
	{
		this.dataSignal.set (response);
	}

	public afterSuccess (_: ActionType, __: TData | null)
	{
		this.stateSignal.set ("idle");
	}

	public handleError (_: ActionType, error: any)
	{
		this.errorSignal.set ({
			statusCode: 500,
			message: `${error}`
		});
	}

	public afterError (_: ActionType, __: any)
	{
		this.stateSignal.set ("idle");
	}

	protected appendToPath (path?: string | null)
	{
		if (!path)
			return this.apiPath;
		return `${this.apiPath}/${path}`;
	}

	protected appendToBasePath (path?: string | null)
	{
		if (!path)
			return this.apiBasePath;
			return `${this.apiBasePath}/${path}`;
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
							this.handleSuccess ("GET", response);
							this.afterSuccess ("GET", response);
						},
						error: (error) =>
						{
							this.handleError ("GET", error);
							this.afterError ("GET", error);
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
							this.handleSuccess ("POST", response);
							this.afterSuccess ("POST", response);
						},
						error: (error) =>
						{
							this.handleError ("POST", error);
							this.afterError ("POST", error);
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
							this.handleSuccess ("PUT", response);
							this.afterSuccess ("PUT", response);
						},
						error: (error) =>
						{
							this.handleError ("PUT", error);
							this.afterError ("PUT", error);
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
							this.handleSuccess ("DELETE", null);
							this.afterSuccess ("DELETE", null);
						},
						error: (error) =>
						{
							this.handleError ("DELETE", error);
							this.afterError ("DELETE", error);
						}
					});
	}
}