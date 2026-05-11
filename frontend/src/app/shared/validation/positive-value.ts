import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export type PositiveValueValidatorArgs = {
  inclusive: boolean;
  allowNull: boolean;
};

export function positiveValueValidator (args?: Partial<PositiveValueValidatorArgs>) : ValidatorFn
{
  const inclusive = args?.inclusive ?? false;
  const allowNull = args?.allowNull ?? false;
  
  return (control: AbstractControl): ValidationErrors | null =>
  {
    const value = control.value;
    const castedValue = +value;
    const numericalValue = Number.isNaN (castedValue) ? null : castedValue;

    if (allowNull && value === null)
      return null;
    if (!allowNull && value === null || numericalValue == null)
      return {noNumericalValue: true};

    if (inclusive && value < 0 || !inclusive && value <= 0)
      return {noPositiveValue: true};
    return null;
  };
}