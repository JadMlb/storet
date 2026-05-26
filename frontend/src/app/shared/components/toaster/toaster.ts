import { Component, inject } from '@angular/core';
import { ToastService } from '../../services/layout/toast-service';
import { Toast } from './toast/toast';

@Component ({
	selector: 'toaster',
	imports: [Toast],
	templateUrl: './toaster.html',
	styleUrl: './toaster.scss',
})
export class Toaster
{
	protected readonly toastService = inject (ToastService);

	public clearToast (id: string) : void
	{
		this.toastService.clearToast (id);
	}
}