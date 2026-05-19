import { DestroyRef, DOCUMENT, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ThemeMode } from '../../types/ThemeMode';
import { UserPreferencesService } from '../user/user-preferences-service';

@Injectable ({
	providedIn: 'root',
})
export class ThemeModeService
{
	private readonly preferences = inject (UserPreferencesService);
	private readonly root = inject (DOCUMENT);
	private readonly destroyRef = inject (DestroyRef);

	private modeSignal = signal<ThemeMode> ("auto");

	public theme = this.modeSignal.asReadonly();

	constructor ()
	{
		this.preferences
			.theme$
			.pipe (takeUntilDestroyed (this.destroyRef))
			.subscribe (
				mode =>
				{
					this.modeSignal.set (mode);
					this.applyTheme (mode);
				}
			);
	}

	private applyTheme (mode: ThemeMode) : void
	{
		this.root.documentElement.style.colorScheme = mode === "auto" ? "" : `only ${mode}`;
	}

	public setTheme (theme?: ThemeMode | null) : void
	{
		if (!theme)
			return;

		this.preferences.setThemePreferences (theme);
	}
}