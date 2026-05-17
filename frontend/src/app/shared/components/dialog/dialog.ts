import { Component, input, output } from '@angular/core';
import { Button } from '../button/button';
import { ButtonRole } from '../button/Role';
	
@Component ({
	selector: 'styled-dialog',
	imports: [Button],
	templateUrl: './dialog.html',
	styleUrl: './dialog.scss',
})
export class Dialog
{
	public open = input.required<boolean>();
	public submissionDisabled = input (false);
	public submitButtonRole = input<ButtonRole> ("primary");
	public cancelButtonRole = input<ButtonRole> ("warn");

	public onSubmitRequest = output<void>();
	public onClose = output<void>();
	
	public close (e: Event) : void
	{
		e.preventDefault();
		e.stopPropagation();
		
		this.onClose.emit();
	}

	public save (e: Event)
	{
		e.stopPropagation();
		e.preventDefault();
		
		this.onSubmitRequest.emit();
	}
}