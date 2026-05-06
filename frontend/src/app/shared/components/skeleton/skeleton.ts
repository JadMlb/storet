import { Component, computed, Input, numberAttribute } from '@angular/core';

@Component ({
  selector: 'skeleton',
  imports: [],
  templateUrl: './skeleton.html',
  styleUrl: './skeleton.scss',
})
export class Skeleton
{
  @Input ({transform: numberAttribute})
  set lines (value: number)
  {
    this._lines = value > 0 ? value : 1;
  }
  
  private _lines: number = 1;

  loops = computed (
    () => Array.from ({length: this._lines}, (_, i) => i)
  )
}