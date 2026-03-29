import { computed, Injectable, signal } from '@angular/core';
import { createClient, Session, SupabaseClient, User } from '@supabase/supabase-js/dist/index.cjs';
import { environment } from '../environments/environment';
import { Credentials } from '../types/Credentials';
import { ReplaySubject } from 'rxjs';

@Injectable ({
  providedIn: 'root',
})
export class AuthService
{
  private readonly supabaseClient: SupabaseClient;
  private userSignal = signal<User | null> (null);
  private sessionSignal = signal<Session | null> (null);
  private initializedSubject = new ReplaySubject<void> (1);
  
  public isAuthenticated = computed (() => !!this.userSignal());
  public initialized = this.initializedSubject.asObservable();
  
  constructor ()
  {
    this.supabaseClient = createClient (environment.authUrl, environment.authKey);
    
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