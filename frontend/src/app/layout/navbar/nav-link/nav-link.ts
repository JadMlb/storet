import { booleanAttribute, Component, input, Input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component ({
	selector: 'nav-link',
	imports: [RouterLink, RouterLinkActive],
	templateUrl: './nav-link.html',
	styleUrl: './nav-link.scss',
})
export class NavLink
{
	to = input.required<string>();
	text = input.required<string>();
	exact = input (false);

	onClick = output<void>();

	handleClick () : void
	{
		this.onClick.emit();
	}
}