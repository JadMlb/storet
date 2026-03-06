import { FormGroup } from "@angular/forms";
import { ActionType, Service } from "./service";
import { ActivatedRoute, Router } from "@angular/router";

export abstract class DetailsService<TData> extends Service<TData>
{
  protected form?: FormGroup;
  protected router?: Router;
  protected route?: ActivatedRoute;
  
  public setForm (form?: FormGroup)
  {
    this.form = form;
    return this;
  }
  
  public setRouting (router?: Router, activatedRoute?: ActivatedRoute)
  {
    this.router = router;
    this.route = activatedRoute;
    return this;
  }
  
  protected abstract mapResponseToFormData (response: any) : any;
  
  override handleSuccess (actionType: ActionType, response: any): void
  {
    super.handleSuccess (actionType, response);
    if (this.form && response && actionType === "GET")
    {
      this.form.patchValue (this.mapResponseToFormData (response));
      this.form.markAsPristine();
    }
    else if (actionType === "POST" || actionType === "DELETE" && response === null)
    {
      this.router?.navigate (
        ["../"],
        {
          relativeTo: this.route,
          state: {refresh: true, timestamp: Date.now()}
        }
      );
    }
  }
}