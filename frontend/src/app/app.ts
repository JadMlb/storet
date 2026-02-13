import { Component } from '@angular/core';
import { Navbar } from './components/navbar/navbar';
import { Contents } from './components/contents/contents';

@Component({
  selector: 'app-root',
  imports: [Navbar, Contents],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
}