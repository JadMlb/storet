import { Component, computed, input } from '@angular/core';

@Component ({
	selector: 'toast-progress',
	imports: [],
	templateUrl: './toast-progress.html',
	styleUrl: './toast-progress.scss',
})
export class ToastProgress
{
	value = input.required<number | null>();

	protected safeValue = computed (
		() => this.value() ? Math.max (0, Math.min (100, this.value()!)) : 0
	);
}