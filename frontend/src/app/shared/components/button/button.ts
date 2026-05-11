import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component ({
  selector: 'styled-button',
  imports: [CommonModule],
  templateUrl: './button.html',
  styleUrl: './button.scss',
})
export class Button
{
  @Input() smallerPadding: boolean = false;
  @Input() smallerBorderRadius: boolean = false;
  @Input() form: string | null = null;
  @Input() disabled: boolean = false;
  @Input() type: "reset" | "submit" | "button" = "button";
  @Input() role: "primary" | "normal" | "warn" = "normal";
  @Input() renderStyle: "filled" | "outlined" = "filled";
  @Input() fill: boolean = false;
  @Output() onClick = new EventEmitter<any>();

  handleClick (event: Event)
  {
    this.onClick.emit (event);
  }
}
