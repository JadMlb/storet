import { Component, inject, signal } from '@angular/core';
import { AuthService } from '../../../services/auth-service';
import { Router } from '@angular/router';
import { Credentials } from '../../../types/Credentials';
import { AuthComponentBase } from '../base/base';

@Component ({
  selector: 'signup',
  imports: [AuthComponentBase],
  templateUrl: './signup.html',
  styleUrl: './signup.scss',
})
export class Signup
{
  private readonly router = inject (Router);
  private readonly authService = inject (AuthService);
  
  error = signal<string | null> (null);
  
  public async handleSubmission (value: Credentials) : Promise<void>
  {
    try
    {
      await this.authService.signUp (value);
      this.error.set (null);
      this.router.navigateByUrl ("/");
    }
    catch (err)
    {
      if (err instanceof Error)
        this.error.set (err.message);
      else
        this.error.set ("An error occured");
    }
  }
  
  public handleSecondaryNavigation () : void
  {
    this.router.navigateByUrl ("/login");
  }
}