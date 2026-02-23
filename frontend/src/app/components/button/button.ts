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
  @Input() className: string | null = null;
  @Input() form: string | null = null;
  @Input() disabled: boolean = false;
  @Input() type: "reset" | "submit" | "button" = "button";
  @Input() role: "primary" | "normal" | "warn" = "normal";
  @Output() onClick = new EventEmitter<any>();

  handleClick (event: Event)
  {
    this.onClick.emit (event);
  }
}
