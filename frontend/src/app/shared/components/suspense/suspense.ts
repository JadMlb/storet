import { booleanAttribute, Component, DestroyRef, inject, input, Input, signal } from '@angular/core';
import { Skeleton } from '../skeleton/skeleton';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { map, of, switchMap, timer } from 'rxjs';

@Component ({
  selector: 'suspense',
  imports: [Skeleton],
  templateUrl: './suspense.html',
  styleUrl: './suspense.scss',
})
export class Suspense
{
  loading = input.required<boolean>();

  protected showSkeleton = signal (false);

  private destroyRef = inject (DestroyRef);

  constructor ()
  {
    const loading$ = toObservable (this.loading);
    loading$.pipe (
      switchMap (
        isLoading =>
        {
          if (!isLoading)
            return of (false);
          return timer (200)
                  .pipe (map (() => true));
        }
      ),
      takeUntilDestroyed (this.destroyRef)
    )
    .subscribe (
      show => this.showSkeleton.set (show)
    );
  }
}