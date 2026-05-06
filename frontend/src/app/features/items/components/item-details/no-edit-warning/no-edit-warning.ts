import { Component, input } from '@angular/core';

@Component ({
  selector: 'no-edit-warning',
  imports: [],
  templateUrl: './no-edit-warning.html',
  styleUrl: './no-edit-warning.scss',
})
export class NoEditWarning
{
  show = input (false);
}