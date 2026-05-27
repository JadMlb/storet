import { HttpClient } from "@angular/common/http";
import { DestroyRef, inject, signal } from "@angular/core";
import { environment } from "../../../../environments/environment";
import { takeUntilDestroyed, toObservable } from "@angular/core/rxjs-interop";
import { Subject } from "rxjs";
import { isError } from "../../types/Error";
import { GetPathProps } from "../../types/PathProps";
import { ServiceEvent } from "../../types/ServiceEvent";

export type ServiceStateType = "loading" | "idle";

export type ActionType = "GET" | "POST" | "PUT" | "DELETE";

export abstract class Service<TData>
{
	private readonly destroyRef = inject (DestroyRef);
	protected readonly http = inject (HttpClient);
	protected apiBasePath: string;
	protected apiPath: string;
	
	protected dataSignal = signal<TData | null> (null);
	protected stateSignal = signal<ServiceStateType> ("idle");
	protected eventsSubject = new Subject<ServiceEvent>();

	constructor (controllerName: string)
	{
		this.apiBasePath = `${environment.backendServerUrl}/api`;
		this.apiPath = `${this.apiBasePath}/${controllerName}`;
		this.destroyRef.onDestroy (
			() => this.eventsSubject.complete()
		);
	}

	public readonly data = this.dataSignal.asReadonly();
	public readonly state = this.stateSignal.asReadonly();
	public readonly data$ = toObservable (this.dataSignal);
	public readonly events$ = this.eventsSubject.asObservable().pipe (takeUntilDestroyed (this.destroyRef));

	public beforeRequest ()
	{
		this.stateSignal.set ("loading");
	}

	public handleSuccess (actionType: ActionType, response: TData | null)
	{
		this.dataSignal.set (response);
		if (actionType === "GET")
			return;
		this.eventsSubject.next ({type: "success"});
	}

	public afterSuccess (_: ActionType, __: TData | null)
	{
		this.stateSignal.set ("idle");
	}

	public handleError (_: ActionType, error: any)
	{
		let err = error.error;
		if (!err || isError (err))
			this.eventsSubject.next ({type: "error", error: err ?? null});
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
}