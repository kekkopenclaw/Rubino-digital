import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { QuoteBuilderComponent } from '../quote-builder/quote-builder.component';
import { PricingService } from '../../services/pricing.service';
import { gbp } from '../../models/pricing.model';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, QuoteBuilderComponent],
  template: `
  <section class="hero"><div class="container">
    <div class="grid grid-2" style="align-items:center">
      <div class="reveal">
        <span class="badge">Websites · Web apps · Custom software</span>
        <h1>We build websites that <em>move</em>.</h1>
        <p>Rubino Digital is a London web development agency. Cinematic, 3D-scroll websites, web apps and custom software — with a clear price before you even talk to us.</p>
        <p><a class="btn" href="#builder">Build my quote</a> <a class="btn btn-outline" href="#portfolio">See our work</a></p>
        <p class="muted">From £349 · 30% deposit · No hidden fees</p>
      </div>
      <div class="cube-stage" aria-hidden="true"><div class="cube"><i></i><i></i><i></i><i></i><i></i><i></i></div>
        <p style="text-align:center" class="muted float">Pure CSS 3D — no 3D library, no download, no licence to worry about.</p></div>
    </div>
  </div></section>

  <section id="services"><div class="container">
    <h2>What we build</h2>
    <div class="grid grid-2">
      <article class="card"><h3>Business Websites</h3><p>Fast, beautiful websites that turn visitors into customers. From <strong>£799</strong>.</p></article>
      <article class="card"><h3>Web Apps</h3><p>Accounts, dashboards, bookings and payments — software that runs your business. From <strong>£2,999</strong>.</p></article>
      <article class="card"><h3>Custom Software</h3><p>Bespoke tools built around how you actually work. From <strong>£4,999</strong>.</p></article>
      <article class="card"><h3>Ready-made Templates</h3><p>Proven designs, customised with your content and launched in days. From <strong>£349</strong>.</p></article>
    </div>
  </div></section>

  <section id="portfolio"><div class="container">
    <h2>Our work</h2>
    <div class="grid grid-2">
      <article class="card"><span class="badge">Web App</span><h3>FileMyReturn</h3>
        <p>Online Self Assessment tax platform — customers create an account, complete their return in the software, pay by card, and the return is checked and filed to HMRC.</p>
        <p class="muted">.NET 10 · Angular 18 · Stripe · From £2,999 as a web app</p></article>
      <article class="card"><span class="badge">Business Website</span><h3>SMASH — Cinematic Burger Site</h3>
        <p>Dark, gold-accented 3D-scroll restaurant site with a burger builder, 90-second sear timer and table reservations.</p>
        <p class="muted">Angular 18 · CSS 3D · Scroll animation · From £799</p></article>
    </div>
  </div></section>

  <section id="products"><div class="container">
    <h2>Ready-made products</h2>
    <p class="muted">Buy the pattern, not just the hours. Each one loads straight into the quote builder.</p>
    <div class="grid grid-3">
      <article class="card"><h3>SMASH Restaurant Template</h3><p>The SMASH burger site as a ready-made template — your menu, photos and branding, live in days.</p><p class="price">{{ gbp(349) }}</p><button class="btn btn-sm" type="button" (click)="addToQuote('template', ['booking','animation-3d'])">Add to quote</button></article>
      <article class="card"><h3>FileMyReturn Platform</h3><p>The FileMyReturn pattern — accounts, forms, payments and an admin dashboard — rebuilt for your service.</p><p class="price">{{ gbp(2999) }}</p><button class="btn btn-sm" type="button" (click)="addToQuote('webapp', ['accounts','admin-dashboard','stripe'])">Add to quote</button></article>
      <article class="card"><h3>Starter Template Pack</h3><p>A landing page plus a business template, with SEO and analytics set up, ready to launch.</p><p class="price">{{ gbp(499) }}</p><button class="btn btn-sm" type="button" (click)="addToQuote('landing', ['seo','analytics'])">Add to quote</button></article>
    </div>
  </div></section>

  <section id="process"><div class="container">
    <h2>How it works</h2>
    <div class="grid grid-2">
      <article class="card"><h3>1. Build your quote</h3><p>Pick a project, add the features you want, and see the price live.</p></article>
      <article class="card"><h3>2. Send your enquiry</h3><p>Send the quote to us in one click. We reply with a start date.</p></article>
      <article class="card"><h3>3. We build it</h3><p>Pay the 30% deposit, we build, and you review it as it takes shape.</p></article>
      <article class="card"><h3>4. You go live</h3><p>Pay the balance on delivery. Hosting, domain and SSL set up for you.</p></article>
    </div>
  </div></section>

  <section id="builder"><div class="container">
    <h2>Build your quote</h2>
    <p class="muted">Add features and watch the price change. Pick 4 or more add-ons and get 10% off the add-ons.</p>
    <app-quote-builder #builder></app-quote-builder>
  </div></section>

  <section id="faq"><div class="container">
    <h2>Questions</h2>
    <details class="card" style="margin-bottom:12px"><summary><strong>How much does a website cost?</strong></summary><p>Templates from £349, landing pages from £499, business websites from £799, e-commerce from £1,499, web apps from £2,999 and custom software from £4,999. Build your exact price in the quote builder above.</p></details>
    <details class="card" style="margin-bottom:12px"><summary><strong>How long does it take?</strong></summary><p>A template takes about a week, a business website about 2–3 weeks, and a web app about 6 weeks. The builder shows your estimated timeline as you add features.</p></details>
    <details class="card" style="margin-bottom:12px"><summary><strong>How do I pay?</strong></summary><p>A 30% deposit secures your start date and the balance is due on delivery. Card payments are by Stripe (test mode until we go live with real payments).</p></details>
    <details class="card"><summary><strong>Do you host the site?</strong></summary><p>Yes — hosting and domain setup is a £150 add-on, and monthly care plans start at £49/month.</p></details>
  </div></section>

  <section id="contact"><div class="container">
    <h2>Contact</h2>
    <p class="muted">Prefer to just talk? Send a message — it goes to the same inbox as quote enquiries.</p>
    <div class="card">
      <label for="c-name">Your name</label><input id="c-name" [(ngModel)]="name" autocomplete="name">
      <label for="c-email">Email</label><input id="c-email" type="email" [(ngModel)]="email" autocomplete="email">
      <label for="c-msg">Message</label><textarea id="c-msg" rows="4" [(ngModel)]="message"></textarea>
      <p><button class="btn" type="button" (click)="sendContact()" [disabled]="sending">Send message</button></p>
      @if (error) { <p class="error" role="alert">{{ error }}</p> }
      @if (ref) { <p class="success">Message sent — reference <strong>{{ ref }}</strong>.</p> }
    </div>
    <p style="margin-top:16px"><a routerLink="/register">Create an account</a> to save quotes and track your enquiries.</p>
  </div></section>`,
})
export class LandingComponent {
  @ViewChild('builder') builder?: QuoteBuilderComponent;
  name = ''; email = ''; message = ''; sending = false; error = ''; ref = '';
  gbp = gbp;
  constructor(private pricing: PricingService) {}
  addToQuote(projectType: string, addOnIds: string[]): void {
    this.builder?.loadPreset(projectType, addOnIds);
  }
  sendContact(): void {
    this.error = ''; this.ref = '';
    if (!this.name.trim() || !this.email.includes('@') || this.message.trim().length < 10) {
      this.error = 'Please enter your name, a valid email, and a message of at least 10 characters.'; return;
    }
    this.sending = true;
    this.pricing.sendEnquiry({ name: this.name.trim(), email: this.email.trim(), message: this.message.trim() }).subscribe({
      next: r => { this.ref = r.reference; this.sending = false; },
      error: () => { this.error = 'Could not send your message right now. Please try again.'; this.sending = false; },
    });
  }
}
