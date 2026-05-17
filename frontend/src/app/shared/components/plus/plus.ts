import { Component, computed, input } from '@angular/core';

@Component ({
	selector: 'plus',
	imports: [],
	templateUrl: './plus.html',
	styleUrl: './plus.scss',
})
export class Plus
{
	color = input<string>();

	colorClass = computed (
		() =>
		{
			return `fill-${this.color()}`
		}
	);
}