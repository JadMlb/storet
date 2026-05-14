import { Component, input, output } from '@angular/core';

@Component ({
  selector: 'drawer',
  imports: [],
  templateUrl: './drawer.html',
  styleUrl: './drawer.scss',
})
export class Drawer
{
  open = input.required<boolean>();

  onClose = output<void>();

  handleClose ()
  {
    this.onClose.emit();
  }

  blockPropagate (e: MouseEvent)
  {
    e.stopPropagation();
  }
}
