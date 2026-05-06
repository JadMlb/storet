import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthComponentBase } from '../base/base';
import { AuthService } from '../../services/auth-service';
import { Credentials } from '../../types/Credentials';

@Component ({
  selector: 'login',
  imports: [AuthComponentBase],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login
{
  private readonly route = inject (ActivatedRoute);
  protected readonly router = inject (Router);
  protected readonly authService = inject (AuthService);
  
  error = signal<string | null> (null);
  
  private returnUrl = computed (
    () =>
    {
      const rawReturnUrl = this.route.snapshot.queryParams["returnUrl"];
      if (!rawReturnUrl)
        return "/";
      
      try
      {
        const decoded = decodeURIComponent (rawReturnUrl);
        if (decoded.startsWith ("/"))
          return decoded;
        return "/";
      }
      catch
      {
        return "/";
      }
    }
  );
  
  public async handleSubmission (value: Credentials) : Promise<void>
  {
    try
    {
      await this.authService.signIn (value);
      this.error.set (null);
      this.router.navigateByUrl (this.returnUrl());
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
    this.router.navigateByUrl ("/signup");
  }
}