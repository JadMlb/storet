import { Component, computed, forwardRef, Input, OnInit, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, SelectControlValueAccessor } from '@angular/forms';
import { Option } from '../../types/Option';
import { Chevron } from '../chevron/chevron';

@Component ({
  selector: 'combobox',
  imports: [Chevron],
  templateUrl: './combobox.html',
  styleUrl: './combobox.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef (() => Combobox),
      multi: true
    }
  ]
})
export class Combobox implements ControlValueAccessor, OnInit
{
  @Input() options: Option[] = [];
  @Input() label: string | null = null;
  @Input() placeholder = "Select an option";
  @Input() required = false;
  disabled = false;

  isOpen = signal (false);
  searchTerm = signal ("");
  selectedId = signal<string | null> (null);
  selectedValue = signal<Option | null> (null);

  chevronRotation = computed (
    () => this.isOpen() ? "top" : "bottom"
  );
  
  filteredOptions = computed (
    () =>
    {
      const term = this.searchTerm().toLowerCase();
      const allOptions = this.options;

      if (!term)
        return allOptions;

      return allOptions.filter (
        opt => opt.display.toLowerCase().includes (term)
      );
    }
  );

  displayText = computed (
    () => this.selectedValue()?.display ?? ""
  );

  showPlaceholder = computed (
    () => this.selectedId() === null
  );

  private onChange: any = () => {};
  private onTouched: any = () => {};

  private updateSelected ()
  {
    const currentValue = this.selectedId();
    const selected = this.options.find (opt => opt.value === currentValue);
    this.selectedValue.set (selected ?? null);
  }

  ngOnInit ()
  {
    this.updateSelected();
  }

  isSelected (option: Option)
  {
    return this.selectedId() === option.value;
  }

  writeValue (value: any): void
  {
    this.selectedId.set (`${value}`);
    this.updateSelected();
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

  toggleDropdown (): void
  {
    if (this.disabled)
      return;

    this.isOpen.update (old => !old);
    if (this.isOpen())
      this.searchTerm.set ("");
  }

  selectOption (option: Option): void
  {
    const currentValue = this.selectedId();
    if (currentValue === option.value && !this.required)
      this.selectedId.set (null);
    else
      this.selectedId.set (option.value);

    this.onChange (this.selectedId());
    this.onTouched();
    this.updateSelected();
    this.isOpen.set (false);
  }

  clearSelection (event: Event): void
  {
    event.stopPropagation();

    if (this.disabled)
      return;

    const newValue = null;
    this.selectedId.set (newValue);
    this.onChange (newValue);
    this.updateSelected();
    this.onTouched();
  }

  search (event: Event): void
  {
    const term = (event.target as HTMLInputElement).value;
    this.searchTerm.set (term);
  }
}