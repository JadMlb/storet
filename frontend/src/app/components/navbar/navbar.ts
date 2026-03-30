import { Component } from '@angular/core';
import { NavLink } from '../nav-link/nav-link';
import { Logout } from '../logout/logout';

@Component ({
	selector: 'navbar',
	imports: [NavLink, Logout],
	templateUrl: './navbar.html',
	styleUrl: './navbar.scss',
})
export class Navbar {

}