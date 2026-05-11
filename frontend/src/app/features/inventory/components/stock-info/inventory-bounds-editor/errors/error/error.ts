import { Component, computed, input } from '@angular/core';

@Component ({
  selector: 'error',
  imports: [],
  templateUrl: './error.html',
  styleUrl: './error.scss',
})
export class Error
{
  source = input.required<string>();
  descriptor = input<string | null>();

  readonly descriptorMap = {
    "form-maxLessThanMin": "less than defined minimum value",
    "max-noPositiveValue": "greater than 0",
    "min-noPositiveValue": "positive number"
  }

  protected errorText = computed (
    () =>
    {
      const source = this.source();
      const descriptor = this.descriptor();
      if (!this.descriptor())
        return "";
      return this.descriptorMap[`${source}-${descriptor}` as keyof typeof this.descriptorMap];
    }
  );

  protected concernedValue = computed (
    () =>
    {
      const source = this.source();
      return ["max", "form"].includes (source) ? "Maximum" : "Minimum";
    }
  );
}