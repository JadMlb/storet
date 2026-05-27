import { FormGroup } from "@angular/forms";
import { ActionType } from "./service";
import { NavigationService } from "../layout/navigation-service";
import { PathProps, PostPathProps } from "../../types/PathProps";
import { UpdatableService } from "./updatable-service";

export abstract class DetailsService<TData> extends UpdatableService<TData>
{
	protected form?: FormGroup;
	protected navigationService?: NavigationService;
	
	public setForm (form?: FormGroup)
	{
		this.form = form;
		return this;
	}
	
	public setRouting (navigationService?: NavigationService)
	{
		this.navigationService = navigationService;
		return this;
	}
	
	public abstract updateForm (response: any) : void;
	
	override handleSuccess (actionType: ActionType, response: any): void
	{
		super.handleSuccess (actionType, response);
		if (this.form && response && actionType === "GET")
		{
			this.updateForm (response);
			this.form.markAsPristine();
		}
		else if (actionType === "POST" || actionType === "PUT" || actionType === "DELETE" && response === null)
		{
			this.navigationService?.navigateBack ({refresh: true, timestamp: Date.now()});
		}
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