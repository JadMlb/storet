import { Component, computed, input } from '@angular/core';
import { StockStatus } from '../../../models/Stock';

const STATUS_TO_COLOUR_MAP = {
  "empty_accepted": "orange",
  "empty_not_accepted": "red",
  "critical": "yellow",
  "sufficient": "green",
  "full": "green"
} as const;

@Component ({
  selector: 'stock-tag',
  imports: [],
  templateUrl: './stock-tag.html',
  styleUrl: './stock-tag.scss',
})
export class StockTag
{
  status = input<StockStatus>();
  readonly label = computed (
    () =>
    {
      const statusFirstPart = this.status()?.split("_")[0] ?? "N/A";
      return statusFirstPart[0].toUpperCase() + statusFirstPart.slice (1);
    }
  );

  protected isColour (colour: "green" | "orange" | "yellow" | "red")
  {
    if (!this.status())
      return false;
    return STATUS_TO_COLOUR_MAP[this.status() as StockStatus] === colour;
  }
}