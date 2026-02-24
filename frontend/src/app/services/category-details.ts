import { Injectable } from '@angular/core';
import { ActionType, Service } from './service';
import { CategoryWithParentType } from '../types/CategoryType';
import { FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

@Injectable ({
  providedIn: 'root',
})
export class CategoryDetailsService extends Service<CategoryWithParentType>
{
  private form?: FormGroup;
  private router?: Router;
  private route?: ActivatedRoute;

  constructor ()
  {
    super ("categories");
  }

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

  override handleSuccess (actionType: ActionType, response: CategoryWithParentType | null): void
  {
    super.handleSuccess (actionType, response);
    if (this.form && response && actionType === "GET")
    {
      this.form.patchValue ({label: response.label, parentCategoryId: response.parentCategory?.id ?? null});
      this.form.markAsPristine();
    }
    else if (response === null)
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
