import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';

@Component ({
  selector: 'drawer',
  imports: [],
  templateUrl: './drawer.html',
  styleUrl: './drawer.scss',
})
export class Drawer
{
  @Input ({transform: booleanAttribute})
  open: boolean = false;

  @Output()
  onClose = new EventEmitter<undefined>();

  handleClose ()
  {
    this.onClose.emit();
  }

  blockPropagate (e: MouseEvent)
  {
    e.stopPropagation();
  }
}
