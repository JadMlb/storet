import { Component, inject, input, output } from '@angular/core';
import { ResponsiveService } from '../../../shared/services/responsive-service';

@Component ({
	selector: 'hamburger',
	imports: [],
	templateUrl: './hamburger.html',
	styleUrl: './hamburger.scss',
})
export class Hamburger
{
	protected readonly responsiveness = inject (ResponsiveService);
	
	open = input (false);
	
	onClick = output<void>();

	handleClick (e: Event) : void
	{
		e.stopPropagation();
		e.preventDefault();

		this.onClick.emit();
	}
}