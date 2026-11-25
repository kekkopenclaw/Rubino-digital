import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface AuthResponse { token: string; userId: string; fullName: string; email: string; emailConfirmed: boolean; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly key = 'rubino_token';
  constructor(private http: HttpClient) {}
  get token(): string | null { return localStorage.getItem(this.key); }
  get isLoggedIn(): boolean { return !!this.token; }
  register(fullName: string, email: string, password: string): Observable<unknown> {
    return this.http.post('/api/auth/register', { fullName, email, password });
  }
  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', { email, password })
      .pipe(tap(r => localStorage.setItem(this.key, r.token)));
  }
  logout(): void { localStorage.removeItem(this.key); this.http.post('/api/auth/logout', {}).subscribe({ error: () => undefined }); }
  forgotPassword(email: string): Observable<unknown> { return this.http.post('/api/auth/forgot-password', { email }); }
  resetPassword(token: string, newPassword: string): Observable<unknown> { return this.http.post('/api/auth/reset-password', { token, newPassword }); }
  verifyEmail(token: string): Observable<unknown> { return this.http.post('/api/auth/verify-email', { token }); }
  me(): Observable<unknown> { return this.http.get('/api/auth/me'); }
  myQuotes(): Observable<unknown[]> { return this.http.get<unknown[]>('/api/quotes/mine'); }
  myEnquiries(): Observable<unknown[]> { return this.http.get<unknown[]>('/api/enquiries/mine'); }
}
