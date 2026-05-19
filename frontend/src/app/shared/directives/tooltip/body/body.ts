import { Component, input, OnInit } from '@angular/core';

@Component ({
	selector: 'tooltip-body',
	imports: [],
	templateUrl: './body.html',
	styleUrl: './body.scss',
})
export class TooltipBody
{
	text = input ("");
	top = input (0);
	left = input (0);
}