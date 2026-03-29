import { Component } from '@angular/core';
import { Navbar } from '../navbar/navbar';
import { Contents } from '../contents/contents';

@Component ({
  selector: 'app-layout',
  imports: [Navbar, Contents],
  templateUrl: './app-layout.html',
  styleUrl: './app-layout.scss',
})
export class AppLayout
{

}