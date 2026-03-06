import { Component, EventEmitter, Output } from '@angular/core';
import { RouterOutlet } from "@angular/router";
import { Button } from '../button/button';

@Component ({
  selector: 'list-view',
  imports: [RouterOutlet, Button],
  templateUrl: './list-view.html',
  styleUrl: './list-view.scss',
})
export class ListView
{
  @Output()
  onCreateRequest = new EventEmitter<void>();
  
  handleCreateRequest ()
  {
    this.onCreateRequest.emit();
  }
}