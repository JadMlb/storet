import { booleanAttribute, Component, Input, Optional, Self } from '@angular/core';
import { ControlValueAccessor, NgControl, Validators } from '@angular/forms';

@Component ({
  selector: 'text-input',
  imports: [],
  templateUrl: './input.html',
  styleUrl: './input.scss',
  providers: []
})
export class TextInput implements ControlValueAccessor
{
  @Input ({transform: booleanAttribute})
  multiline: boolean = false;
  
  @Input()
  label: string = "";

  @Input()
  placeholder: string = "";

  onChange: any = () => {};
  onTouched: any = () => {};
  disabled = false;

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
}