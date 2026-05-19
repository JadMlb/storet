import { computed, DestroyRef, inject, Injectable, signal } from '@angular/core';

@Injectable ({
	providedIn: 'root',
})
export class ResponsiveService
{
	private readonly destroyRef = inject (DestroyRef);
	private readonly isDesktopMediaQuery = window.matchMedia (`(min-width: 640px)`);
	private isDesktopSignal = signal (this.isDesktopMediaQuery.matches);

	public isDesktop = this.isDesktopSignal.asReadonly();
	public isSmallScreen = computed (
		() => !this.isDesktopSignal()
	);

	constructor ()
	{
		const handleMediaChange = (e: MediaQueryListEvent) =>
		{
			this.isDesktopSignal.set (e.matches);
		}

		this.isDesktopMediaQuery.addEventListener ("change", handleMediaChange);
		this.destroyRef.onDestroy (
			() =>
			{
				this.isDesktopMediaQuery.removeEventListener ("change", handleMediaChange);
			}
		);
	}
}