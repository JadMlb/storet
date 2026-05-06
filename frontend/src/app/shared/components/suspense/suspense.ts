import { booleanAttribute, Component, Input } from '@angular/core';
import { Skeleton } from '../skeleton/skeleton';

@Component ({
  selector: 'suspense',
  imports: [Skeleton],
  templateUrl: './suspense.html',
  styleUrl: './suspense.scss',
})
export class Suspense
{
  @Input ({transform: booleanAttribute})
  loading: boolean = false;
}