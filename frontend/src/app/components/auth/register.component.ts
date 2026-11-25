import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
  <section><div class="container" style="max-width:520px">
    <h2>Create an account</h2>
    <p class="muted">Save quotes and track your enquiries. Your password must be at least 8 characters and include a letter and a number.</p>
    <div class="card">
      <label for="name">Full name</label><input id="name" [(ngModel)]="fullName" autocomplete="name">
      <label for="email">Email</label><input id="email" type="email" [(ngModel)]="email" autocomplete="email">
      <label for="password">Password</label><input id="password" type="password" [(ngModel)]="password" autocomplete="new-password">
      <p><button class="btn" type="button" (click)="register()" [disabled]="busy">Create account</button></p>
      @if (error) { <p class="error" role="alert">{{ error }}</p> }
      @if (done) { <p class="success">Account created. Please check your email to verify your address, then <a routerLink="/login">log in</a>.</p> }
      <p>Already have an account? <a routerLink="/login">Log in</a></p>
    </div>
  </div></section>`,
})
export class RegisterComponent {
  fullName = ''; email = ''; password = ''; error = ''; done = false; busy = false;
  constructor(private auth: AuthService) {}
  register(): void {
    this.error = ''; this.done = false; this.busy = true;
    this.auth.register(this.fullName.trim(), this.email.trim(), this.password).subscribe({
      next: () => { this.done = true; this.busy = false; },
      error: (e) => { this.error = e?.error?.error ?? 'Could not create your account. Please try again.'; this.busy = false; },
    });
  }
}
