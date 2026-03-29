import { Component, input } from '@angular/core';

@Component ({
  selector: 'eye',
  imports: [],
  templateUrl: './eye.html',
  styleUrl: './eye.scss',
})
export class Eye
{
  closed = input (false);
}