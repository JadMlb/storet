import { Component, EventEmitter, forwardRef, Input, Output } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component ({
  selector: 'text-input',
  imports: [],
  templateUrl: './input.html',
  styleUrl: './input.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef (() => TextInput),
      multi: true
    }
  ]
})
export class TextInput implements ControlValueAccessor
{
  @Input()
  label: string = "";

  @Input()
  placeholder: string = "";

  onChange: any = () => {};
  onTouched: any = () => {};
  disabled = false;

  value: string = "";

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
}