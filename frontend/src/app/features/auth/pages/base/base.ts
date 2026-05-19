import { Component, computed, DestroyRef, inject, input, OnInit, output, signal } from '@angular/core';
import { AbstractControl, FormControl, FormRecord, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { TextInput } from '../../../../shared/components/text-input/text-input';
import { PasswordInput } from '../../../../shared/components/password-input/password-input';
import { Button } from '../../../../shared/components/button/button';
import { Credentials } from '../../types/Credentials';
import { ResponsiveService } from '../../../../shared/services/layout/responsive-service';

function patternValidator (regex: RegExp, error: ValidationErrors): ValidatorFn
{
	return (control: AbstractControl): ValidationErrors | null =>
	{
		if (!control.value)
			return null;
			
		return regex.test (control.value) ? null : error;
	};
}

function passwordValidator () : ValidatorFn[]
{
	return [
		Validators.required,
		patternValidator (/\d/, {hasNumber: true}),
		patternValidator (/[A-Z]/, {hasCapitalCase: true}),
		patternValidator (/[a-z]/, {hasSmallCase: true}),
		patternValidator (/[!@#$%^&*]/, {hasSpecialCharacters: true}),
		Validators.minLength (8)
	];
}

function passwordMatchValidator (formGroup: AbstractControl) : ValidationErrors | null
{
	const password = formGroup.get("password")?.value;
	const confirmPassword = formGroup.get("confirmPassword")?.value;
	
	if (confirmPassword && password && password !== confirmPassword)
		formGroup.get("confirmPassword")?.setErrors ({passwordMatches: true});
		
	return null;
}

@Component ({
	selector: 'auth-base',
	imports: [ReactiveFormsModule, TextInput, PasswordInput, Button],
	templateUrl: './base.html',
	styleUrl: './base.scss',
})
export class AuthComponentBase implements OnInit
{
	readonly destroyRef = inject (DestroyRef);
	private readonly reponsiveness = inject (ResponsiveService);
	
	confirmPassword = input (false);
	error = input<string | null> (null);
	
	onSubmit = output<Credentials>();
	onSecondaryNavigation = output();

	buttonPaddingBlock = computed (
		() => this.reponsiveness.isSmallScreen() ? "medium" : "xsmall"
	);
	
	primaryButtonDisabled = signal (true);
	form = new FormRecord (
		{
			email: new FormControl<string | null> (null, [Validators.required, Validators.email]),
			password: new FormControl<string | null> (null, [Validators.required, ...passwordValidator()]),
		},
		{validators: [passwordMatchValidator]}
	);
	
	public ngOnInit () : void
	{
		if (this.confirmPassword())
			this.form.addControl ("confirmPassword", new FormControl<string | null> (null, [Validators.required]));
			
		this.form.statusChanges.subscribe (
			status => this.primaryButtonDisabled.set (status !== "VALID")
		);
	}
	
	public handleFormSubmit () : void
	{
		if (this.form.invalid)
			return;
		
		const value = this.form.value;
		const mappedValue = {
			email: value["email"]!,
			password: value["password"]!,
		} satisfies Credentials;
		if (Object.values(mappedValue).some (v => !v))
			return;
		
		this.onSubmit.emit (mappedValue);
	}
	
	public navigate (e: Event) : void
	{
		e.preventDefault();
		e.stopPropagation();
		
		this.onSecondaryNavigation.emit();
	}
}