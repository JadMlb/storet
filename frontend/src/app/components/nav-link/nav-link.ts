import { booleanAttribute, Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component ({
  selector: 'nav-link',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './nav-link.html',
  styleUrl: './nav-link.scss',
})
export class NavLink
{
  @Input() to!: string;
  @Input() text!: string;
  @Input ({transform: booleanAttribute}) exact = false; 
}