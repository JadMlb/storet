import { computed, inject, Injectable, signal } from '@angular/core';
import { Supabase } from '../../../shared/services/user/supabase';
import { Session, User } from '@supabase/supabase-js/dist/index.cjs';
import { Credentials } from '../types/Credentials';
import { ReplaySubject } from 'rxjs';

@Injectable ({
	providedIn: 'root',
})
export class AuthService
{
	private readonly supabase = inject (Supabase);
	private readonly supabaseClient = this.supabase.getClient();
	private userSignal = signal<User | null> (null);
	private sessionSignal = signal<Session | null> (null);
	private initializedSubject = new ReplaySubject<void> (1);
	
	public isAuthenticated = computed (() => !!this.userSignal());
	public initialized = this.initializedSubject.asObservable();
	
	constructor ()
	{
		this.supabaseClient.auth.onAuthStateChange (
			(event, session) =>
			{
				this.sessionSignal.set (session);
				this.userSignal.set (session?.user ?? null);
				
				if (event === "INITIAL_SESSION")
				{
					this.initializedSubject.next();
					this.initializedSubject.complete();
				}
			}
		);
	}
	
	public getUserEmail () : string
	{
		return this.userSignal()?.email ?? "";
	}
	
	public getAccessToken () : string | null
	{
		return this.sessionSignal()?.access_token ?? null;
	}
	
	async signIn (credentials: Credentials) : Promise<void>
	{
		const {error} = await this.supabaseClient.auth.signInWithPassword (credentials);
		if (error)
			throw error;
	}
	
	async signUp (credentials: Credentials) : Promise<void>
	{
		const {error} = await this.supabaseClient.auth.signUp (credentials);
		if (error)
			throw error;
	}
	
	async signOut () : Promise<void>
	{
		await this.supabaseClient.auth.signOut();
	}
}