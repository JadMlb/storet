import { Component, computed, input, Optional, Self, signal } from '@angular/core';
import { ControlValueAccessor, NgControl, Validators } from '@angular/forms';
import { InputBase } from '../input-base/input-base';

@Component ({
	selector: 'number-input',
	imports: [InputBase],
	templateUrl: './number-input.html',
	styleUrl: './number-input.scss',
})
export class NumberInput implements ControlValueAccessor
{
	label = input ("");
	step = input (1);
	min = input (-Number.MAX_VALUE);
	max = input (Number.MAX_VALUE);

	onChange: any = () => {};
	onTouched: any = () => {};
	disabled = signal (false);

	value = signal (0);
	
	incrementDisabled = computed (() => this.disabled() || this.value() === this.max());
	decrementDisabled = computed (() => this.disabled() || this.value() === this.min());

	constructor (@Self() @Optional() private parent?: NgControl)
	{
		if (this.parent)
			this.parent.valueAccessor = this;
	}

	public get isRequired (): boolean
	{
		return Boolean (this.parent?.control?.hasValidator (Validators.required));
	}
	
	private canSetValue (newValue: any) : boolean
	{
		const newNumberValue = this.safelyCastValueToNumber (newValue);
		return newNumberValue >= this.min() && newNumberValue <= this.max();
	}
	
	public increment (event: Event) : void
	{
		event.preventDefault();
		event.stopPropagation();
		
		const newValue = this.value() + this.step();
		if (!this.canSetValue (newValue))
			return;
		this.value.set (newValue);
		this.onChange (newValue);
	}
	
	public decrement (event: Event) : void
	{
		event.preventDefault();
		event.stopPropagation();
		
		const newValue = this.value() - this.step();
		if (!this.canSetValue (newValue))
			return;
		this.value.set (newValue);
		this.onChange (newValue);
	}
	
	private safelyCastValueToNumber (value: any) : number
	{
		var castedValue = +value;
		if (Number.isNaN (castedValue))
			return 0;
		return castedValue;
	}

	writeValue (value: any): void
	{
		this.value.set (this.safelyCastValueToNumber (value));
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
		this.disabled.set (isDisabled);
	}

	onInput (event: Event)
	{
		const inputValue = this.safelyCastValueToNumber ((event.target as HTMLInputElement).value)
		this.value.set (inputValue);
		this.onChange (inputValue);
	}

	onBlur ()
	{
		this.onTouched();
	}
}
