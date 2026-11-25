import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PricingService } from '../../services/pricing.service';
import { ADD_ONS, CARE_PLANS, DELIVERY_OPTIONS, PROJECT_TYPES, calculateQuote, gbp, QuoteResult } from '../../models/pricing.model';

@Component({
  selector: 'app-quote-builder',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
  <div class="grid grid-2" style="align-items:start">
    <div>
      <h3>1. Pick your project</h3>
      <div class="grid" role="radiogroup" aria-label="Project type">
        @for (t of projectTypes; track t.id) {
          <label class="card" style="cursor:pointer; padding:16px; margin:0">
            <input type="radio" name="projectType" [value]="t.id" [(ngModel)]="projectType" (change)="recalc()" [attr.aria-label]="t.name">
            <strong>{{ t.name }}</strong> — {{ gbp(t.price) }}
            <span class="muted" style="display:block; font-weight:400">{{ t.description }}</span>
          </label>
        }
      </div>

      <h3 style="margin-top:24px">2. Add features <span class="muted" style="font-weight:400">— pick 4 or more and get 10% off add-ons</span></h3>
      <div class="grid grid-2">
        @for (a of addOns; track a.id) {
          <label class="card" style="cursor:pointer; padding:14px; margin:0">
            <input type="checkbox" [checked]="selectedAddOns.includes(a.id)" (change)="toggleAddOn(a.id)" [attr.aria-label]="a.name">
            <strong>{{ a.name }}</strong> — {{ gbp(a.price) }}
            <span class="muted" style="display:block; font-weight:400">{{ a.description }}</span>
          </label>
        }
      </div>

      <h3 style="margin-top:24px">3. Extra pages &amp; delivery</h3>
      <p>Extra pages — {{ gbp(120) }} each</p>
      <p>
        <button class="btn btn-outline btn-sm" type="button" (click)="changePages(-1)" [disabled]="extraPages <= 0" aria-label="Remove a page">−</button>
        <strong style="margin:0 12px" aria-live="polite">{{ extraPages }} extra {{ extraPages === 1 ? 'page' : 'pages' }}</strong>
        <button class="btn btn-outline btn-sm" type="button" (click)="changePages(1)" [disabled]="extraPages >= 50" aria-label="Add a page">+</button>
      </p>
      <label for="delivery">Delivery speed</label>
      <select id="delivery" [(ngModel)]="delivery" (change)="recalc()">
        @for (d of deliveryOptions; track d.id) { <option [value]="d.id">{{ d.label }}</option> }
      </select>
      <label for="care">Monthly care plan (optional)</label>
      <select id="care" [(ngModel)]="carePlan" (change)="recalc()">
        <option value="">No care plan</option>
        @for (c of carePlans; track c.id) { <option [value]="c.id">{{ c.name }} — {{ gbp(c.monthlyPrice) }}/month</option> }
      </select>
    </div>

    <div class="card" style="position:sticky; top:76px" aria-live="polite">
      <h3>Your quote</h3>
      <p><strong>{{ result.projectTypeName }}</strong> — {{ gbp(result.basePrice) }}</p>
      @if (result.extraPages > 0) { <p>Extra pages ({{ result.extraPages }}) — {{ gbp(result.extraPagesCost) }}</p> }
      @for (a of result.addOns; track a.id) { <p>{{ a.name }} — {{ gbp(a.price) }}</p> }
      @if (result.bundleDiscount > 0) { <p>Bundle discount (10% off add-ons) — −{{ gbp(result.bundleDiscount) }}</p> }
      @if (result.deliverySurcharge > 0) { <p>{{ result.delivery }} delivery — +{{ gbp(result.deliverySurcharge) }}</p> }
      <hr>
      <p class="price">{{ gbp(result.total) }}</p>
      <p>Deposit today (30%): <strong>{{ gbp(result.deposit) }}</strong><br>Balance on delivery: <strong>{{ gbp(result.balance) }}</strong></p>
      <p>Estimated timeline: <strong>{{ result.timelineWeeks }} {{ result.timelineWeeks === 1 ? 'week' : 'weeks' }}</strong></p>
      @if (result.carePlanMonthly > 0) { <p>Care plan: <strong>{{ gbp(result.carePlanMonthly) }}/month</strong> (not included in the total above)</p> }
      <p>
        <button class="btn" type="button" (click)="copyQuote()">Copy quote</button>
        <button class="btn btn-outline" type="button" (click)="downloadQuote()">Download (.txt)</button>
      </p>
      @if (copied) { <p class="success">Quote copied to your clipboard.</p> }
      <h4>Send this quote as an enquiry</h4>
      <label for="q-name">Your name</label>
      <input id="q-name" [(ngModel)]="name" autocomplete="name">
      <label for="q-email">Email</label>
      <input id="q-email" type="email" [(ngModel)]="email" autocomplete="email">
      <label for="q-msg">Anything else? (optional)</label>
      <textarea id="q-msg" rows="3" [(ngModel)]="message"></textarea>
      <p><button class="btn" type="button" (click)="sendEnquiry()" [disabled]="sending">Send enquiry</button></p>
      @if (error) { <p class="error" role="alert">{{ error }}</p> }
      @if (enquiryRef) { <p class="success">Enquiry sent — reference <strong>{{ enquiryRef }}</strong>. We will reply as soon as we can.</p> }
      @if (savedRef) { <p class="muted">Saved quote reference: <strong>{{ savedRef }}</strong></p> }
    </div>
  </div>`,
})
export class QuoteBuilderComponent {
  projectTypes = PROJECT_TYPES;
  addOns = ADD_ONS;
  carePlans = CARE_PLANS;
  deliveryOptions = DELIVERY_OPTIONS;
  projectType = 'business';
  extraPages = 0;
  selectedAddOns: string[] = [];
  delivery = 'standard';
  carePlan = '';
  result: QuoteResult = calculateQuote('business', 0, [], 'standard', null);
  name = ''; email = ''; message = '';
  copied = false; sending = false; error = ''; enquiryRef = ''; savedRef = '';
  gbp = gbp;

  constructor(private pricing: PricingService) {}

  /** Called by "Add to quote" buttons on products. */
  loadPreset(projectType: string, addOnIds: string[]): void {
    this.projectType = projectType;
    this.selectedAddOns = [...addOnIds];
    this.recalc();
    document.getElementById('builder')?.scrollIntoView({ behavior: 'smooth' });
  }

  toggleAddOn(id: string): void {
    this.selectedAddOns = this.selectedAddOns.includes(id)
      ? this.selectedAddOns.filter(x => x !== id)
      : [...this.selectedAddOns, id];
    this.recalc();
  }
  changePages(delta: number): void {
    this.extraPages = Math.max(0, Math.min(50, this.extraPages + delta));
    this.recalc();
  }
  recalc(): void {
    this.result = calculateQuote(this.projectType, this.extraPages, this.selectedAddOns, this.delivery, this.carePlan || null);
  }
  quoteText(): string {
    const r = this.result;
    const lines = [
      `Rubino Digital — Quote`,
      `${r.projectTypeName} — ${gbp(r.basePrice)}`,
      ...(r.extraPages > 0 ? [`Extra pages (${r.extraPages}) — ${gbp(r.extraPagesCost)}`] : []),
      ...r.addOns.map(a => `${a.name} — ${gbp(a.price)}`),
      ...(r.bundleDiscount > 0 ? [`Bundle discount — -${gbp(r.bundleDiscount)}`] : []),
      ...(r.deliverySurcharge > 0 ? [`${r.delivery} delivery — +${gbp(r.deliverySurcharge)}`] : []),
      `Total: ${gbp(r.total)}`,
      `Deposit (30%): ${gbp(r.deposit)}`,
      `Balance: ${gbp(r.balance)}`,
      `Estimated timeline: ${r.timelineWeeks} weeks`,
      ...(r.carePlanMonthly > 0 ? [`Care plan: ${gbp(r.carePlanMonthly)}/month`] : []),
    ];
    return lines.join('\n');
  }
  async copyQuote(): Promise<void> {
    try { await navigator.clipboard.writeText(this.quoteText()); this.copied = true; }
    catch { this.error = 'Copy did not work in this browser — use Download instead.'; }
  }
  downloadQuote(): void {
    const blob = new Blob([this.quoteText()], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url; a.download = 'rubino-digital-quote.txt'; a.click();
    URL.revokeObjectURL(url);
  }
  sendEnquiry(): void {
    this.error = ''; this.enquiryRef = '';
    if (!this.name.trim() || !this.email.includes('@') || this.email.length < 5) {
      this.error = 'Please enter your name and a valid email address.'; return;
    }
    this.sending = true;
    const body = { projectType: this.projectType, extraPages: this.extraPages, addOnIds: this.selectedAddOns, delivery: this.delivery, carePlan: this.carePlan || null };
    // Save the quote on the server first (server re-calculates the price),
    // then send the enquiry linked to that saved quote reference.
    this.pricing.saveQuote(body).subscribe({
      next: saved => {
        this.savedRef = saved.reference;
        this.pricing.sendEnquiry({
          name: this.name.trim(), email: this.email.trim(),
          message: (this.message.trim() || 'Please send me this quote.') + '\n\n' + this.quoteText(),
          quoteReference: saved.reference, quoteSummary: this.quoteText(),
        }).subscribe({
          next: enq => { this.enquiryRef = enq.reference; this.sending = false; },
          error: () => { this.error = 'The quote was saved, but sending the enquiry failed. Please try again.'; this.sending = false; },
        });
      },
      error: () => { this.error = 'Could not save your quote right now. Please try again.'; this.sending = false; },
    });
  }
}
