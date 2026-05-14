import { inject, Injectable, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, NavigationExtras, Router } from '@angular/router';
import { filter } from 'rxjs';

@Injectable ({
  providedIn: 'root',
})
export class NavigationService
{
  private readonly activatedRoute = inject (ActivatedRoute);
  private readonly router = inject (Router);
  private idSignal = signal<string | null> (null);

  public id = this.idSignal.asReadonly();

  constructor ()
  {
    this.updateId();

    this.router
        .events
        .pipe (filter (event => event instanceof NavigationEnd))
        .subscribe (() => this.updateId());
  }

  private updateId () : void
  {
    let child = this.activatedRoute.snapshot.firstChild;
    while (child?.firstChild)
      child = child.firstChild;

    const id = child?.paramMap.get ("id") ?? null;
    this.idSignal.set (id);
  }

  public navigateBack (state?: NavigationExtras["state"]) : void
  {
    const currentUrl = this.router.url;
    const parentUrl = currentUrl.slice (0, currentUrl.lastIndexOf ("/"));
    
    this.router.navigateByUrl (parentUrl, {state});
  }
}
