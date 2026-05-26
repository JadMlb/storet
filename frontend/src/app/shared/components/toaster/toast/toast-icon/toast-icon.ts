import { Component, input } from '@angular/core';
import { ToastType } from '../../../../types/Toast';

@Component ({
	selector: 'toast-icon',
	imports: [],
	templateUrl: './toast-icon.html',
	styleUrl: './toast-icon.scss',
})
export class ToastIcon
{
	type = input.required<ToastType>();
}