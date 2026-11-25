import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { calculateQuote, QuoteResult } from '../models/pricing.model';

@Injectable({ providedIn: 'root' })
export class PricingService {
  constructor(private http: HttpClient) {}
  /** Local calculation — instant, works without the backend. */
  calculate(projectType: string, extraPages: number, addOnIds: string[], delivery: string, carePlan: string | null): QuoteResult {
    return calculateQuote(projectType, extraPages, addOnIds, delivery, carePlan);
  }
  /** Server calculation — the authoritative price, used when saving a quote. */
  calculateOnServer(body: { projectType: string; extraPages: number; addOnIds: string[]; delivery: string; carePlan?: string | null }): Observable<QuoteResult> {
    return this.http.post<QuoteResult>('/api/quotes/calculate', body);
  }
  saveQuote(body: { projectType: string; extraPages: number; addOnIds: string[]; delivery: string; carePlan?: string | null }): Observable<{ reference: string }> {
    return this.http.post<{ reference: string }>('/api/quotes', { quote: body });
  }
  getPricing(): Observable<unknown> {
    return this.http.get('/api/pricing').pipe(catchError(() => of(null)));
  }
  getPortfolio(): Observable<unknown> { return this.http.get('/api/portfolio').pipe(catchError(() => of(null))); }
  getProducts(): Observable<unknown> { return this.http.get('/api/products').pipe(catchError(() => of(null))); }
  sendEnquiry(body: { name: string; email: string; phone?: string; message: string; quoteReference?: string; quoteSummary?: string }): Observable<{ reference: string }> {
    return this.http.post<{ reference: string }>('/api/enquiries', body);
  }
}
