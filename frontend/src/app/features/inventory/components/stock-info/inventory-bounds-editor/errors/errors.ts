import { Component, computed, input } from '@angular/core';
import { ValidationErrors } from '@angular/forms';
import { Error } from './error/error';

@Component ({
  selector: 'errors',
  imports: [Error],
  templateUrl: './errors.html',
  styleUrl: './errors.scss',
})
export class Errors
{
  errors = input<string[][]> ([]);

  protected hasErrors = computed (
    () =>
    {
      const errors = this.errors();
      return errors.some (sourceErrorPair => sourceErrorPair[1]);
    }
  );
}