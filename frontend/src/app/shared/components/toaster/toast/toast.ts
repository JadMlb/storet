import { Component, computed, inject, input, output } from '@angular/core';
import { ToastContentsType } from '../../../types/Toast';
import { ToastIcon } from './toast-icon/toast-icon';
import { ToastService } from '../../../services/layout/toast-service';
import { AsyncPipe } from '@angular/common';
import { ToastProgress } from './toast-progress/toast-progress';

@Component ({
	selector: 'toast',
	imports: [ToastIcon, ToastProgress, AsyncPipe],
	templateUrl: './toast.html',
	styleUrl: './toast.scss',
})
export class Toast
{
	private readonly toastsService = inject (ToastService);
	
	public contents = input.required<ToastContentsType>();

	public onClear = output<string>();

	protected hasDescription = computed (() => !!this.contents().description);
	protected type = computed (() => this.contents().type ?? "info");
	protected hasCountDown = computed (() => !!this.contents().autoClears)

	protected get progress$ ()
	{
		return this.toastsService.getToastProgressObservable(this.contents().id)!;
	}

	public handleClearRequest (e: Event) : void
	{
		e.preventDefault();
		e.stopPropagation();

		this.onClear.emit (this.contents().id);
	}
}