import { inject, Injectable, signal } from '@angular/core';
import { Supabase } from './supabase';
import { ThemeMode } from '../../types/ThemeMode';
import { ReplaySubject } from 'rxjs';

@Injectable ({
	providedIn: 'root',
})
export class UserPreferencesService
{
	private readonly supabase = inject (Supabase);
	private readonly supabaseClient = this.supabase.getClient();

	private readonly themePreferencesSubject = new ReplaySubject<ThemeMode> (1);
	private readonly currentTheme = signal<ThemeMode> ("auto");

	public theme$ = this.themePreferencesSubject.asObservable();

	constructor ()
	{
		this.supabaseClient.auth.onAuthStateChange (
			(event, session) =>
			{
				if (["INITIAL_SESSION", "TOKEN_REFRESHED", "USER_UPDATED"].includes (event))
				{
					const preferences = session?.user.user_metadata;
					let theme : ThemeMode = "auto";
					if (preferences && ["auto", "dark", "light"].includes (preferences["theme"]))
						theme = preferences["theme"];

					this.themePreferencesSubject.next (theme);
					this.currentTheme.set (theme);
				}
			}
		);
	}

	public async setThemePreferences (theme: ThemeMode) : Promise<void>
	{
		if (theme === this.currentTheme())
			return;

		const {error} = await this.supabaseClient.auth.updateUser ({data: {theme}});

		if (error)
			throw error;
	}
}