import { Component, computed, input, OnChanges, OnInit, output, signal, SimpleChanges } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, PristineChangeEvent, ReactiveFormsModule, StatusChangeEvent, ValidationErrors, Validators, ValueChangeEvent } from '@angular/forms';
import { NumberInput } from '../../../../../shared/components/number-input/number-input';
import { Switch } from '../../../../../shared/components/switch/switch';
import { StockBounds, StockWithBounds } from '../../../models/Stock';
import { positiveValueValidator } from '../../../../../shared/validation/positive-value';
import { Errors } from './errors/errors';
import { Dialog } from '../../../../../shared/components/dialog/dialog';

function minLessThanMax (control: AbstractControl) : ValidationErrors | null
{
  const min = control.get("min")!.value;
  const max = control.get("max")!.value;

  if (max && max < min)
    return {maxLessThanMin: true};
  return null;
}

@Component ({
  selector: 'inventory-bounds-editor',
  imports: [Dialog, ReactiveFormsModule, NumberInput, Switch, Errors],
  templateUrl: './inventory-bounds-editor.html',
  styleUrl: './inventory-bounds-editor.scss',
})
export class InventoryBoundsEditor implements OnInit, OnChanges
{
  public open = input.required<boolean>();
  public itemId = input.required<string>();
  public itemName = input<string | null>();
  public bounds = input<StockWithBounds | null>();
  public onSave = output<StockBounds>();
  public onClose = output<void>();

  protected boundsForm = new FormGroup (
    {
      min: new FormControl (0, [Validators.required, positiveValueValidator ({inclusive: true})]),
      max: new FormControl<number | null> (null, [positiveValueValidator ({allowNull: true})]),
      hasMaxValue: new FormControl<boolean> (false)
    },
    [minLessThanMax]
  );

  private formPristine = signal (true);
  private formInvalid = signal (true);
  private noRealChangesHappened = signal (true);
  protected submissionDisabled = computed (
    () =>
    {
      const noRealChangesHappened = this.noRealChangesHappened();
      const formInvalid = this.formInvalid();
      const formPristine = this.formPristine();

      return formPristine || formInvalid || noRealChangesHappened;
    }
  );

  protected get errors ()
  {
    return [
      ["form", Object.keys (this.boundsForm.errors ?? {})?.[0] ?? null],
      ["min", Object.keys (this.boundsForm.controls.min.errors ?? {})?.[0] ?? null],
      ["max", Object.keys (this.boundsForm.controls.max.errors ?? {})?.[0] ?? null]
    ];
  }

  public close () : void
  {
    this.onClose.emit();
  }

  private mapFormValueToStockBounds () : StockBounds
  {
    return {
      minQuantity: this.boundsForm.value.min!,
      maxQuantity: this.boundsForm.value.max
    };
  }

  public save ()
  {
    if (!this.boundsForm.valid)
      return;

    this.onSave.emit (this.mapFormValueToStockBounds());
    this.close();
  }

  private patchBoundsValue () : void
  {
    this.boundsForm.patchValue ({
      hasMaxValue: Boolean (this.bounds()?.maxQuantity ?? false),
      min: this.bounds()?.minQuantity ?? 0,
      max: this.bounds()?.maxQuantity
    });
  }

  ngOnChanges (changes: SimpleChanges) : void
  {
    if (changes["open"]?.currentValue)
      this.patchBoundsValue();
  }

  ngOnInit () : void
  {
    this.boundsForm
        .controls
        .hasMaxValue
        .valueChanges
        .subscribe (
          value =>
          {
            this.boundsForm.patchValue ({
              max: value ? 0 : null
            });
          }
        );

    this.boundsForm
        .events
        .subscribe (
          event =>
          {
            if (event instanceof ValueChangeEvent)
            {
              const providedValue = this.bounds();
              const formValue = event.value;
              this.noRealChangesHappened.set (!!providedValue && providedValue.minQuantity === formValue.min && providedValue.maxQuantity === formValue.max);
            }
            
            if (event instanceof StatusChangeEvent)
              this.formInvalid.set (event.status === "INVALID");
  
            if (event instanceof PristineChangeEvent)
              this.formPristine.set (event.pristine);
          }
        );
  }
}
