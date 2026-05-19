import { Injectable } from '@angular/core';
import { createClient, SupabaseClient } from '@supabase/supabase-js';
import { environment } from '../../../../environments/environment';

@Injectable ({
	providedIn: 'root',
})
export class Supabase
{
	private readonly supabaseClient: SupabaseClient;

	constructor ()
	{
		this.supabaseClient = createClient (environment.authUrl, environment.authKey);
	}

	public getClient () : SupabaseClient
	{
		return this.supabaseClient;
	}
}