import { Component, inject } from '@angular/core';
import { Navbar } from './navbar/navbar';
import { Contents } from './contents/contents';
import { ThemeModeService } from '../shared/services/layout/theme-mode-service';

@Component ({
  selector: 'app-layout',
  imports: [Navbar, Contents],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.scss',
})
export class AppLayout
{
	private readonly theme = inject (ThemeModeService);
}