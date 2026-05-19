import { Component, computed, input, output } from '@angular/core';
import { Dialog } from '../../dialog/dialog';
import { TooltipDirective } from '../../../directives/tooltip/tooltip';

@Component ({
	selector: 'confirm-delete',
	imports: [Dialog, TooltipDirective],
	templateUrl: './confirm-delete.html',
	styleUrl: './confirm-delete.scss',
})
export class ConfirmDelete
{
	open = input.required<boolean>();
	entity = input<string>();
	instance = input<string | null>();
	instanceLabel = input<string | null>();

	onDelete = output<void>();
	onCancel = output<void>();

	deleteText = computed (
		() =>
		{
			const entity = this.entity();
			return this.instanceLabel() || (entity ? `this ${entity}` : "instance")
		}
	);

	handleDelete () : void
	{
		this.onDelete.emit();
	}

	handleCancel () : void
	{
		this.onCancel.emit();
	}
}