import { Component, computed, ElementRef, HostListener, inject, signal } from '@angular/core';
import { AuthService } from '../../features/auth/services/auth-service';
import { Button } from '../../shared/components/button/button';
import { Router } from '@angular/router';
import { ModeSwitcher } from './mode-switcher/mode-switcher';

@Component ({
	selector: 'profile',
	imports: [Button, ModeSwitcher],
	templateUrl: './profile.html',
	styleUrl: './profile.scss',
})
export class Profile
{
	private readonly router = inject (Router);
	private readonly authService = inject (AuthService);
	private ref = inject (ElementRef);
	
	menuOpen = signal (false);
	user = computed (
		() =>
		{
			const email = this.authService.getUserEmail();
			return email.slice (0, email.indexOf ("@"));
		}
	);
	userInitial = computed (
		() =>
		{
			const user = this.user();
			return user.charAt(0).toUpperCase();
		}
	);
	
	toggleMenuOpen () : void
	{
		this.menuOpen.update (old => !old);
	}
	
	changeMode () : void
	{
		
	}
	
	logout () : void
	{
		this.authService.signOut();
		this.router.navigateByUrl ("login");
	}
	
	@HostListener ("document:click", ["$event"])
	closeOnClickOutside (event: Event) : void
	{
		if (!this.ref.nativeElement.contains (event.target))
			this.menuOpen.set (false);
	}
}