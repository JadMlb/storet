import { DestroyRef, inject, Injectable, OnInit, signal } from "@angular/core";
import { FormGroup } from "@angular/forms";
import { NavigationService } from "../services/navigation-service";
import { toObservable } from "@angular/core/rxjs-interop";

@Injectable()
export abstract class ListViewDetailsFormLogicBase implements OnInit
{
  protected destroyRef = inject (DestroyRef);
  protected readonly creating = signal (false);
  protected readonly editing = signal (false);
  protected editsHappened = false;
  
  protected readonly navigation = inject (NavigationService);
  
  protected form!: FormGroup;

  constructor ()
  {
    const navigationId = toObservable (this.navigation.id);
    navigationId.subscribe (
      value =>
      {
        if (!value)
        {
          this.creating.set (false);
          this.editing.set (false);
          this.editsHappened = false;
          this.form.reset();
          return;
        }
        
        const isCreating = value === "new";
        this.creating.set (isCreating);
        this.initialiseData (isCreating);
      }
    );
  }
  
  ngOnInit () : void
  {
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

  public initialiseData (creating: boolean = false) : void
  {
    if (creating)
    {
      this.onEditingEnabled();
      this.executeOnInitIfCreating();
    }
    else
    {
      this.executeOnInitIfNotCreating();
      this.form.disable();
    }
  }
  
  protected abstract executeOnInitIfNotCreating () : void;
  protected abstract executeOnInitIfCreating () : void;
  protected abstract shouldMarkFormAsPristine (value: any) : boolean;
  
  onEditingEnabled ()
  {
    this.form.enable();
    this.editing.set (true);
  }
  
  protected abstract resetFormOnCancelEditing () : void;
  
  onCancel () : void
  {
    if (this.creating())
    {
      this.navBack();
      return;
    }

    this.resetFormOnCancelEditing();
    this.form.disable();
    this.editing.set (false);
  }
  
  navBack (): void
  {
    this.navigation.navigateBack ({refresh: this.editsHappened, timestamp: Date.now()});
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