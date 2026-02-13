import { Component } from '@angular/core';
import { NavLink } from '../nav-link/nav-link';

@Component ({
	selector: 'navbar',
	imports: [NavLink],
	templateUrl: './navbar.html',
	styleUrl: './navbar.scss',
})
export class Navbar {

}