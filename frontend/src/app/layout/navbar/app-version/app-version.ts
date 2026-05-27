import { Component } from '@angular/core';

import { version } from "../../../../../package.json";

@Component ({
	selector: 'app-version',
	imports: [],
	templateUrl: './app-version.html',
	styleUrl: './app-version.scss',
})
export class AppVersion
{
	readonly version = version;
}