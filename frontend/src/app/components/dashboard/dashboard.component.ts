import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
  <section><div class="container">
    <h2>Your dashboard</h2>
    @if (me) { <p class="muted">Signed in as {{ (me | json) }}</p> }
    <h3>Your saved quotes</h3>
    @if (quotes.length === 0) { <p class="muted">No saved quotes yet. <a routerLink="/">Build one</a> and send it as an enquiry.</p> }
    @for (q of quotes; track q.reference) {
      <div class="card" style="margin-bottom:12px"><strong>{{ q.reference }}</strong> — {{ q.projectType }} — £{{ q.total }} · Deposit £{{ q.deposit }} · {{ q.timelineWeeks }} weeks · {{ q.status }}</div>
    }
    <h3>Your enquiries</h3>
    @if (enquiries.length === 0) { <p class="muted">No enquiries yet.</p> }
    @for (e of enquiries; track e.reference) {
      <div class="card" style="margin-bottom:12px"><strong>{{ e.reference }}</strong> — {{ e.status }} · {{ e.createdAt }}</div>
    }
  </div></section>`,
})
export class DashboardComponent implements OnInit {
  me: unknown = null; quotes: any[] = []; enquiries: any[] = [];
  constructor(private auth: AuthService) {}
  ngOnInit(): void {
    this.auth.me().subscribe({ next: v => this.me = v, error: () => undefined });
    this.auth.myQuotes().subscribe({ next: v => this.quotes = v, error: () => undefined });
    this.auth.myEnquiries().subscribe({ next: v => this.enquiries = v, error: () => undefined });
  }
}
