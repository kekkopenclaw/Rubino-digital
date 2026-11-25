import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
  <section><div class="container" style="max-width:520px">
    <h2>Log in</h2>
    <p class="muted">Demo account: demo&#64;rubinodigital.example / Demo123!</p>
    <div class="card">
      <label for="email">Email</label><input id="email" type="email" [(ngModel)]="email" autocomplete="email">
      <label for="password">Password</label><input id="password" type="password" [(ngModel)]="password" autocomplete="current-password">
      <p><button class="btn" type="button" (click)="login()" [disabled]="busy">Log in</button></p>
      @if (error) { <p class="error" role="alert">{{ error }}</p> }
      <p><a routerLink="/register">Create an account</a></p>
    </div>
  </div></section>`,
})
export class LoginComponent {
  email = ''; password = ''; error = ''; busy = false;
  constructor(private auth: AuthService, private router: Router) {}
  login(): void {
    this.error = ''; this.busy = true;
    this.auth.login(this.email.trim(), this.password).subscribe({
      next: () => this.router.navigate(['/dashboard']),
      error: (e) => { this.error = e?.error?.error ?? 'Login failed. Check your email and password.'; this.busy = false; },
    });
  }
}
