import { Component, input } from '@angular/core';
import { FieldLabel } from '../field-label/field-label';

@Component ({
	selector: 'input-base',
	imports: [FieldLabel],
	templateUrl: './input-base.html',
	styleUrl: './input-base.scss',
})
export class InputBase
{
	required = input (false);
	disabled = input (false);
	label = input<string | null>();
	resize = input (false);
}