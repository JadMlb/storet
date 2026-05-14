import { FormGroup } from "@angular/forms";
import { ActionType, Service } from "./service";
import { NavigationService } from "./navigation-service";

export abstract class DetailsService<TData> extends Service<TData>
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
}