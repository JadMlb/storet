import { Component, computed, EventEmitter, inject, output, Output } from '@angular/core';
import { Button } from '../button/button';
import { ResponsiveService } from '../../services/responsive-service';
import { Plus } from '../plus/plus';

@Component ({
	selector: 'list-view',
	imports: [Button, Plus],
	templateUrl: './list-view.html',
	styleUrl: './list-view.scss',
})
export class ListView
{
	protected readonly responsiveness = inject (ResponsiveService);
	onCreateRequest = output<void>();

	buttonBorder = computed (
		() => this.responsiveness.isSmallScreen() ? "large" : "medium"
	);
	buttonInlinePadding = computed (
		() => this.responsiveness.isSmallScreen() ? "medium" : "small"
	);
	buttonBlockPadding = computed (
		() => this.responsiveness.isSmallScreen() ? "medium" : "xsmall"
	);
	
	handleCreateRequest ()
	{
		this.onCreateRequest.emit();
	}
}