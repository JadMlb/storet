import { CommonModule } from '@angular/common';
import { Component, computed, input, output } from '@angular/core';
import { ButtonRole } from './Role';

type Size = "xsmall" | "small" | "medium" | "large";

@Component ({
	selector: 'styled-button',
	imports: [CommonModule],
	templateUrl: './button.html',
	styleUrl: './button.scss',
})
export class Button
{
	paddingInline = input<Size> ("medium");
	paddingBlock = input<Size> ("xsmall");
	borderRadius = input<Size> ("small");
	form = input<string | null> (null);
	disabled = input (false);
	type = input<"reset" | "submit" | "button"> ("button");
	role = input<ButtonRole> ("normal");
	renderStyle = input<"filled" | "outlined"> ("filled");
	fill = input (false);

	onClick = output<Event>();

	paddingAndBorderClasses = computed (
		() => `padding-block-${this.paddingBlock()} padding-inline-${this.paddingInline()} border-${this.borderRadius()}`
	);

	handleClick (event: Event)
	{
		this.onClick.emit (event);
	}
}
