import { Component, computed, ElementRef, HostListener, inject, signal } from '@angular/core';
import { NavLink } from './nav-link/nav-link';
import { Profile } from '../profile/profile';
import { Hamburger } from './hamburger/hamburger';
import { ResponsiveService } from '../../shared/services/layout/responsive-service';

@Component ({
	selector: 'navbar',
	imports: [NavLink, Profile, Hamburger],
	templateUrl: './navbar.html',
	styleUrl: './navbar.scss',
})
export class Navbar
{
	protected readonly responsiveness = inject (ResponsiveService);
	private readonly ref = inject (ElementRef);

	navbarOpenWhenOnMobile = signal (false);
	navbarOpen = computed (
		() => this.responsiveness.isDesktop() || this.navbarOpenWhenOnMobile()
	);

	toggleNavbarOpenState () : void
	{
		this.navbarOpenWhenOnMobile.update (old => !old);
	}

	onLinkClick () : void
	{
		if (this.responsiveness.isSmallScreen() && this.navbarOpenWhenOnMobile())
			this.navbarOpenWhenOnMobile.set (false);
	}

	@HostListener ("document:click", ["$event"])
	handleClickOutside (e: Event) : void
	{
		if (!this.ref.nativeElement.contains (e.target))
			this.navbarOpenWhenOnMobile.set (false);
	}
}