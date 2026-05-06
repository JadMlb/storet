import { Component, computed, Input, Optional, Self, signal } from '@angular/core';
import { ControlValueAccessor, NgControl, Validators } from '@angular/forms';
import { Eye } from './eye/eye';
import { FieldLabel } from '../field-label/field-label';

@Component ({
  selector: 'password-input',
  imports: [Eye, FieldLabel],
  templateUrl: './password-input.html',
  styleUrl: './password-input.scss',
})
export class PasswordInput implements ControlValueAccessor
{
  @Input()
  label: string = "";

  @Input()
  placeholder: string = "";

  onChange: any = () => {};
  onTouched: any = () => {};
  disabled = false;

  valueShown = signal (false);
  inputType = computed (() => this.valueShown() ? "text" : "password");

  value: string = "";

  constructor (@Self() @Optional() private parent?: NgControl)
  {
    if (this.parent)
      this.parent.valueAccessor = this;
  }

  public get isRequired (): boolean
  {
    return Boolean (this.parent?.control?.hasValidator (Validators.required));
  }

  writeValue (value: string): void
  {
    this.value = value;
  }

  registerOnChange (fn: any): void
  {
    this.onChange = fn;
  }

  registerOnTouched (fn: any): void
  {
    this.onTouched = fn;
  }

  setDisabledState? (isDisabled: boolean): void
  {
    this.disabled = isDisabled;
  }

  onInput (event: Event)
  {
    this.value = (event.target as HTMLInputElement).value;
    this.onChange (this.value);
  }

  onBlur ()
  {
    this.onTouched();
  }

  toggleVisibility (e: Event) : void
  {
    e.preventDefault();
    e.stopPropagation();

    this.valueShown.update (old => !old);
  }
}
