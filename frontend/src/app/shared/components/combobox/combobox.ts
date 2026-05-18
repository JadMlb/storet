import { Component, computed, input, Optional, Self, signal } from '@angular/core';
import { ControlValueAccessor, NgControl, Validators } from '@angular/forms';
import { Option } from '../../types/Option';
import { Chevron } from '../chevron/chevron';
import { InputBase } from '../input-base/input-base';

@Component ({
	selector: 'combobox',
	imports: [InputBase, Chevron],
	templateUrl: './combobox.html',
	styleUrl: './combobox.scss',
	providers: []
})
export class Combobox implements ControlValueAccessor
{
	options = input<Option[]> ([]);
	label = input<string | null> (null);
	placeholder = input ("Select an option");
	required = input (false);
	multiple = input (false);
	disabled = false;

	isOpen = signal (false);
	searchTerm = signal ("");
	selectedId = signal<string | string[] | null> (null);
	selectedValue = computed (
		() =>
		{
			const opts = this.options();
			const currentValue = this.selectedId();

			if (opts.length === 0)
				return null;

			if (this.multiple())
				return opts.filter (opt => currentValue?.includes (opt.value));
			return opts.find (opt => opt.value === currentValue) ?? null;
		}
	);

	chevronRotation = computed (
		() => this.isOpen() ? "top" : "bottom"
	);

	filteredOptions = computed (
		() =>
		{
			const term = this.searchTerm().toLowerCase();
			const allOptions = this.options();

			if (!term)
				return allOptions;

			return allOptions.filter (
				opt => opt.display.toLowerCase().includes (term)
			);
		}
	);

	displayValues = computed (
		() =>
		{
			const opts = this.options();
			const value = this.selectedValue();
			if (opts.length === 0 || value === null)
				return [];
			return this.multiple() ?
						value as Option[] :
							[value as Option]
		}
	);

	showPlaceholder = computed (
		() => this.multiple() ? this.selectedId()?.length === 0 : this.selectedId() === null
	);

	private onChange: any = () => {};
	private onTouched: any = () => {};

	constructor (@Self() @Optional() private parent?: NgControl)
	{
		if (this.parent)
			this.parent.valueAccessor = this;
	}

	public get isRequired (): boolean
	{
		return Boolean (this.parent?.control?.hasValidator (Validators.required));
	}

	isSelected (option: Option)
	{
		if (this.multiple())
			return this.selectedId()?.includes (option.value);
		return this.selectedId() === option.value;
	}

	private writeValueMultiple (value: any) : void
	{
		if (!this.multiple || !Array.isArray (value) || !value)
			this.selectedId.set ([]);
		else
			this.selectedId.set (value.map ((i: any) => `${i}`));
	}

	private writeValueSingle (value: any) : void
	{
		if (this.multiple() || Array.isArray (value) || !value)
			this.selectedId.set (null);
		else
			this.selectedId.set (`${value}`);
	}

	writeValue (value: any): void
	{
		if (this.multiple())
			this.writeValueMultiple (value);
		else
			this.writeValueSingle (value)
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

	private handleSelectionSingle (option: Option) : void
	{
		const currentValue = this.selectedId() as string | null;
		if (currentValue === option.value && !this.required())
			this.selectedId.set (null);
		else
			this.selectedId.set (option.value);
	}

	private handleSelectionMultiple (option: Option) : void
	{
		const currentValue = this.selectedId() as string[];
		if (currentValue.find (o => option.value === o))
			this.selectedId.update (old => (old as string[]).filter (o => o !== option.value));
		else
			this.selectedId.update (old => [...(old as string[]), option.value]);
	}

	selectOption (option: Option): void
	{
		if (this.multiple())
			this.handleSelectionMultiple (option);
		else
			this.handleSelectionSingle (option);

		this.onChange (this.selectedId());
		this.onTouched();
		this.isOpen.set (false);
	}

	removeItem (event: Event, option: Option)
	{
		event.preventDefault();
		event.stopPropagation();

		this.selectOption (option);
	}

	clearSelection (event: Event): void
	{
		event.stopPropagation();
		event.preventDefault();

		if (this.disabled)
			return;

		const newValue = this.multiple() ? [] : null;
		this.selectedId.set (newValue);
		this.onChange (newValue);
		this.onTouched();
	}

	search (event: Event): void
	{
		const term = (event.target as HTMLInputElement).value;
		this.searchTerm.set (term);
	}
}
