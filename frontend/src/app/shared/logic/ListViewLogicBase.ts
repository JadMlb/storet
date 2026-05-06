import { DestroyRef, inject, Injectable, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { ActivatedRoute, NavigationEnd, Router } from "@angular/router";
import { filter } from "rxjs";

@Injectable()
export abstract class ListViewLogicBase
{
  private readonly router = inject (Router);
  private readonly route = inject (ActivatedRoute);
  private destroyRef = inject (DestroyRef);

  protected drawerOpen = signal (false);

  protected abstract onRefresh () : void;

  private refreshIfNeeded (): void
  {
    const navigation = this.router.currentNavigation();
    if (navigation?.extras?.state?.["refresh"])
      this.onRefresh();
  }

  ngOnInit (): void
  {
    this.refreshIfNeeded();

    this.router.events
                .pipe (
                  filter (event => event instanceof NavigationEnd),
                  takeUntilDestroyed (this.destroyRef)
                )
                .subscribe (
                  () =>
                  {
                    this.refreshIfNeeded();
                  }
                );
  }

  open (): void
  {
    this.drawerOpen.set (true);
  }

  closeDrawer (): void
  {
    this.drawerOpen.set (false);
  }

  navigateToDetails (e: MouseEvent): void
  {
    this.router.navigate ([(e.currentTarget as Element).id], {relativeTo: this.route});
  }

  navigateToNew (): void
  {
    this.router.navigate (["new"], {relativeTo: this.route});
  }
}
