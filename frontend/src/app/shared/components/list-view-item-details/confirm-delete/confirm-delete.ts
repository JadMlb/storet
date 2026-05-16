import { Component, input, output } from '@angular/core';
import { Dialog } from '../../dialog/dialog';

@Component ({
	selector: 'confirm-delete',
	imports: [Dialog],
	templateUrl: './confirm-delete.html',
	styleUrl: './confirm-delete.scss',
})
export class ConfirmDelete
{
	open = input.required<boolean>();
	entity = input<string>();

	onDelete = output<void>();
	onCancel = output<void>();

	handleDelete () : void
	{
		this.onDelete.emit();
	}

	handleCancel () : void
	{
		this.onCancel.emit();
	}
}