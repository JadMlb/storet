import { DestroyRef, inject, Injectable, OnInit, signal } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { ActivatedRoute, Router } from "@angular/router";

@Injectable()
export abstract class ListViewDetailsFormLogicBase implements OnInit
{
  protected destroyRef = inject (DestroyRef);
  protected readonly creating = signal (false);
  protected readonly editing = signal (false);
  protected editsHappened = false;
  
  protected readonly activatedRoute = inject (ActivatedRoute);
  protected readonly router = inject (Router);
  protected readonly id = this.activatedRoute.snapshot.paramMap.get ("id");
  
  protected form!: FormGroup;
  
  ngOnInit () : void
  {
    this.creating.set (this.activatedRoute.snapshot.url[0].path === "new");
    if (this.creating())
    {
      this.editing.set (true);
      this.executeOnInitIfCreating();
    }
    else
    {
      this.executeOnInitIfNotCreating();
      this.form.disable();
    }
    this.subscribeToFormChanges();
  }
  
  private subscribeToFormChanges (): void
  {
    this.form.valueChanges
              .subscribe (
                value =>
                {
                  if (this.shouldMarkFormAsPristine (value))
                  {
                    this.form.markAsPristine();
                    this.editsHappened = false;
                  }
                  else
                    this.editsHappened = true;
                }
              );
  }
  
  protected abstract executeOnInitIfNotCreating () : void;
  protected abstract executeOnInitIfCreating () : void;
  protected abstract shouldMarkFormAsPristine (value: any) : boolean;
  
  onEditingEnabled ()
  {
    this.form.enable();
    this.editing.set (true);
  }
  
  onCancel () : void
  {
    if (this.creating())
    {
      this.navBack();
      return;
    }

    this.form.disable();
    this.editing.set (false);
  }
  
  navBack (): void
  {
    this.router.navigate (
      ["../"],
      {
        relativeTo: this.activatedRoute,
        state: {refresh: this.editsHappened, timestamp: Date.now()}
      }
    );
  }
  
  onFormSubmit ()
  {
    if (!this.form.valid)
      return;
      
    this.handleFormSubmission();
  }
  
  protected abstract handleFormSubmission() : void;
  
  public abstract onItemDelete () : void;
}