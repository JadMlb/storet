import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivateFn, RedirectCommand, Router } from '@angular/router';
import { AuthService } from '../services/auth-service';
import { of, switchMap } from 'rxjs';

function buildUrlFromParts (route: ActivatedRouteSnapshot)
{
  let url = route.pathFromRoot
                .map (
                  v => v.url.map (segment => segment.toString())
                            .join ("/")
                )
                .join ("/")
                .replace (/\/+/g, "/");
  
  const queryParams = route.queryParamMap;
  if (queryParams.keys.length > 0)
  {
    const query = queryParams.keys
                              .map (
                                key => queryParams.getAll (key)
                                                  .map (value => `${key}=${value}`)
                                                  .join ("&")
                              )
                              .join ("&");
    return `${url}?${query}`;
  }
  
  return url || "/";
}

export const authGuard: CanActivateFn = (route, _) =>
{
  const authService = inject (AuthService);
  const router = inject (Router);
  
  return authService.initialized.pipe (
    switchMap (
      () =>
      {
        if (authService.isAuthenticated())
          return of (true);
          
        const returnUrl = encodeURIComponent (buildUrlFromParts (route));
        return of (router.parseUrl (`/login?returnUrl=${returnUrl}`));
      }
    )
  );
};
