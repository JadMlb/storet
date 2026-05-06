import { Component, input } from '@angular/core';

@Component ({
  selector: 'field-label',
  imports: [],
  templateUrl: './field-label.html',
  styleUrl: './field-label.scss',
})
export class FieldLabel
{
  text = input<string | null>();
  required = input<boolean> (false);
}