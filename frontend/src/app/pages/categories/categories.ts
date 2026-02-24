import { Component, DestroyRef, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { Suspense } from '../../components/suspense/suspense';
import { ActivatedRoute, NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Category } from '../../components/category/category';
import { CategoriesService } from '../../services/categories';

@Component ({
  selector: 'categories',
  imports: [Category, Suspense, RouterOutlet],
  templateUrl: './categories.html',
  styleUrl: './categories.scss',
})
export class Categories implements OnInit
{
  private readonly router = inject (Router);
  private readonly route = inject (ActivatedRoute);
  readonly categoriesStore = inject (CategoriesService);
  private destroyRef = inject (DestroyRef);

  drawerOpen = signal (false);

  private refreshIfNeeded ()
  {
    const navigation = this.router.currentNavigation();
    if (navigation?.extras?.state?.["refresh"])
      this.categoriesStore.get();
  }

  ngOnInit ()
  {
    this.categoriesStore.get();

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

  open ()
  {
    this.drawerOpen.set (true);
  }

  closeDrawer ()
  {
    this.drawerOpen.set (false);
  }

  navigateToDetails (e: MouseEvent)
  {
    this.router.navigate ([(e.currentTarget as Element).id], {relativeTo: this.route});
  }
}
