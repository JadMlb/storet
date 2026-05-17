import { Component, computed, inject, input, output } from '@angular/core';
import { Button } from '../button/button';
import { ButtonRole } from '../button/Role';
import { ResponsiveService } from '../../services/responsive-service';
	
@Component ({
	selector: 'styled-dialog',
	imports: [Button],
	templateUrl: './dialog.html',
	styleUrl: './dialog.scss',
})
export class Dialog
{
	private readonly responsiveness = inject (ResponsiveService);
	
	public open = input.required<boolean>();
	public submissionDisabled = input (false);
	public submitButtonRole = input<ButtonRole> ("primary");
	public cancelButtonRole = input<ButtonRole> ("warn");

	public onSubmitRequest = output<void>();
	public onClose = output<void>();

	protected buttonPaddingBlock = computed (
		() => this.responsiveness.isSmallScreen() ? "medium" : "xsmall"
	);
	
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