import { ApplicationRef, ComponentRef, Directive, DOCUMENT, ElementRef, inject, input, inputBinding, ViewContainerRef } from '@angular/core';
import { TooltipBody } from './body/body';

@Directive ({
	standalone: true,
	selector: '[tooltip]',
	host: {
		"(mouseenter)": "onMouseEnter()",
		"(mouseleave)": "onMouseLeave()"
	}
})
export class TooltipDirective
{
	private readonly el = inject (ElementRef);
	private readonly document = inject (DOCUMENT);
	private readonly container = inject (ViewContainerRef);
	private readonly app = inject (ApplicationRef);
	private component: ComponentRef<TooltipBody> | null = null;

	tooltip = input ("");

	onMouseEnter () : void
	{
		if (this.component)
			return;

		const parentBoundingBox = this.el.nativeElement.getBoundingClientRect();
		const top = parentBoundingBox.top + parentBoundingBox.height / 2;
		const left = parentBoundingBox.left + parentBoundingBox.width / 2;

		this.component = this.container.createComponent (
			TooltipBody,
			{
				bindings: [
					inputBinding ("text", this.tooltip),
					inputBinding ("top", () => top),
					inputBinding ("left", () => left)
				]
			}
		);
		this.document.body.appendChild (this.component.location.nativeElement);
		this.component.hostView.detectChanges();
	}

	onMouseLeave () : void
	{
		if (!this.component)
			return;

		this.app.detachView (this.component.hostView);
		this.component.destroy();
		this.component = null;
	}
}
