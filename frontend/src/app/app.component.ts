import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
  <header class="topbar"><div class="container">
    <a class="logo" routerLink="/">Rubino <span>Digital</span></a>
    <nav aria-label="Main">
      <a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{exact:true}">Home</a>
      <a href="#services">Services</a>
      <a href="#portfolio">Work</a>
      <a href="#products">Ready-made</a>
      <a href="#builder">Quote builder</a>
      <a href="#faq">FAQ</a>
      <a href="#contact">Contact</a>
      @if (auth.isLoggedIn) { <a routerLink="/dashboard">Dashboard</a><a href="#" (click)="logout($event)">Log out</a> }
      @else { <a routerLink="/login">Log in</a> }
    </nav>
  </div></header>
  <main><router-outlet></router-outlet></main>
  <footer><div class="container">
    <p><strong>Rubino Digital</strong> — Websites, web apps and custom software. London, UK.</p>
    <p>Prices include VAT. A 30% deposit secures your start date; the balance is due on delivery.</p>
    <p><a routerLink="/login">Client login</a> · <a routerLink="/register">Create an account</a></p>
  </div></footer>`,
})
export class AppComponent {
  constructor(public auth: AuthService) {}
  logout(e: Event): void { e.preventDefault(); this.auth.logout(); location.href = '/'; }
}
