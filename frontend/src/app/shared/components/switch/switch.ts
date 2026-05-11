import { Component, input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl, Validators } from '@angular/forms';

@Component ({
  selector: 'switch',
  imports: [],
  templateUrl: './switch.html',
  styleUrl: './switch.scss',
})
export class Switch implements ControlValueAccessor
{
  label = input<string | null>();
  
  onChange: any = () => {};
  onTouched: any = () => {};
  disabled = false;

  value: boolean = false;

  constructor (@Self() @Optional() private parent?: NgControl)
  {
    if (this.parent)
      this.parent.valueAccessor = this;
  }

  public get isRequired (): boolean
  {
    return Boolean (this.parent?.control?.hasValidator (Validators.required));
  }

  writeValue (value: boolean): void
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

  touch ()
  {
    this.onTouched();
  }

  toggle ()
  {
    if (this.disabled)
      return;
    
    this.value = !this.value;
    this.onChange (this.value);
  }
}