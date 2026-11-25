// Mirrors backend PricingService + pricing.json EXACTLY. Change a price in
// one place only if you also change the other two — see README.md.
export interface ProjectType { id: string; name: string; price: number; baseWeeks: number; description: string; }
export interface AddOn { id: string; name: string; price: number; description: string; }
export interface CarePlan { id: string; name: string; monthlyPrice: number; description: string; }
export interface QuoteLine { id: string; name: string; price: number; }
export interface QuoteResult {
  projectType: string; projectTypeName: string; basePrice: number;
  extraPages: number; extraPagesCost: number;
  addOns: QuoteLine[]; addOnsSubtotal: number; bundleDiscount: number;
  delivery: string; deliveryMultiplier: number; deliverySurcharge: number;
  subtotal: number; total: number; deposit: number; balance: number;
  carePlan: string | null; carePlanMonthly: number; timelineWeeks: number; currency: string;
}
export const PROJECT_TYPES: ProjectType[] = [
  { id: 'template', name: 'Template', price: 349, baseWeeks: 1, description: 'A ready-made design, customised with your content and branding.' },
  { id: 'landing', name: 'Landing Page', price: 499, baseWeeks: 1, description: 'One focused page designed to turn visitors into enquiries or sales.' },
  { id: 'business', name: 'Business Website', price: 799, baseWeeks: 2, description: 'A complete multi-page website for your business.' },
  { id: 'ecommerce', name: 'E-commerce', price: 1499, baseWeeks: 4, description: 'An online shop with products, checkout and payments.' },
  { id: 'webapp', name: 'Web App', price: 2999, baseWeeks: 6, description: 'A custom web application with accounts, dashboards and workflows.' },
  { id: 'custom', name: 'Custom Software', price: 4999, baseWeeks: 8, description: 'Bespoke software built around how your business actually works.' },
];
export const ADD_ONS: AddOn[] = [
  { id: 'ecommerce-payments', name: 'E-commerce & online payments', price: 400, description: 'Sell products and take payments online.' },
  { id: 'booking', name: 'Booking / reservation system', price: 300, description: 'Let customers book a table, appointment or slot online.' },
  { id: 'cms-blog', name: 'CMS & blog', price: 150, description: 'Edit your own pages and publish blog posts.' },
  { id: 'seo', name: 'SEO setup', price: 200, description: 'Search-friendly pages, titles and sitemap so Google can find you.' },
  { id: 'branding', name: 'Logo & branding', price: 250, description: 'Logo, colours and a simple brand kit.' },
  { id: 'animation-3d', name: '3D scroll & cinematic animation', price: 350, description: 'Scroll-driven 3D effects like the SMASH burger site.' },
  { id: 'accounts', name: 'User accounts & login', price: 400, description: 'Customers or staff can sign up, log in and manage their details.' },
  { id: 'admin-dashboard', name: 'Admin dashboard', price: 500, description: 'A private dashboard to manage orders, bookings or content.' },
  { id: 'stripe', name: 'Stripe payments integration', price: 300, description: 'Secure card payments with Stripe (test mode until you go live).' },
  { id: 'mobile-app', name: 'Mobile app version', price: 800, description: 'Your site as an app-style mobile experience.' },
  { id: 'copywriting', name: 'Copywriting', price: 180, description: 'Clear, professional wording for your pages.' },
  { id: 'analytics', name: 'Analytics & tracking', price: 120, description: 'See how many visitors you get and what they do.' },
  { id: 'hosting', name: 'Hosting & domain setup', price: 150, description: 'Domain, hosting and SSL set up for you.' },
  { id: 'multilingual', name: 'Multilingual', price: 250, description: 'Your site in a second language.' },
  { id: 'live-chat', name: 'Live chat & support', price: 180, description: 'Visitors can message you straight from the site.' },
  { id: 'integrations', name: 'Third-party integrations', price: 350, description: 'Connect a CRM, email tool or other service you already use.' },
  { id: 'accessibility', name: 'Accessibility & performance', price: 120, description: 'Fast loading and usable for everyone, including screen readers.' },
];
export const CARE_PLANS: CarePlan[] = [
  { id: 'essential', name: 'Essential Care', monthlyPrice: 49, description: 'Hosting checks, updates and small fixes.' },
  { id: 'growth', name: 'Growth Care', monthlyPrice: 99, description: 'Everything in Essential, plus monthly content updates and reports.' },
  { id: 'premium', name: 'Premium Care', monthlyPrice: 199, description: 'Everything in Growth, plus priority support and new features each month.' },
];
export const EXTRA_PAGE_PRICE = 120;
export const DELIVERY_OPTIONS = [
  { id: 'standard', name: 'Standard', multiplier: 1.0, label: 'Standard — no extra charge' },
  { id: 'fast', name: 'Fast', multiplier: 1.15, label: 'Fast — +15%' },
  { id: 'rush', name: 'Rush', multiplier: 1.3, label: 'Rush — +30%' },
];

/** Client-side mirror of PricingService.Calculate — same rule order, same rounding.
 *  Works offline / without the backend; the server re-calculates on save. */
export function calculateQuote(projectTypeId: string, extraPages: number, addOnIds: string[], deliveryId: string, carePlanId: string | null): QuoteResult {
  const type = PROJECT_TYPES.find(t => t.id === projectTypeId) ?? PROJECT_TYPES[2];
  const delivery = DELIVERY_OPTIONS.find(d => d.id === deliveryId) ?? DELIVERY_OPTIONS[0];
  const pages = Math.max(0, Math.min(50, extraPages));
  const addOns: QuoteLine[] = [...new Set(addOnIds)]
    .map(id => ADD_ONS.find(a => a.id === id)).filter((a): a is AddOn => !!a)
    .map(a => ({ id: a.id, name: a.name, price: a.price }));
  const extraPagesCost = pages * EXTRA_PAGE_PRICE;
  const addOnsSubtotal = addOns.reduce((s, a) => s + a.price, 0);
  const bundleDiscount = addOns.length >= 4 ? Math.round(addOnsSubtotal * 0.10 * 100) / 100 : 0;
  const subtotal = type.price + extraPagesCost + (addOnsSubtotal - bundleDiscount);
  const deliverySurcharge = Math.round(subtotal * (delivery.multiplier - 1) * 100) / 100;
  const total = Math.round(subtotal * delivery.multiplier * 100) / 100;
  const deposit = Math.round(total * 0.30 * 100) / 100;
  const balance = Math.round((total - deposit) * 100) / 100;
  const care = carePlanId ? CARE_PLANS.find(c => c.id === carePlanId) ?? null : null;
  const timelineWeeks = type.baseWeeks + Math.ceil(pages / 4) + Math.ceil(addOns.length / 4);
  return { projectType: type.id, projectTypeName: type.name, basePrice: type.price,
    extraPages: pages, extraPagesCost, addOns, addOnsSubtotal, bundleDiscount,
    delivery: delivery.id, deliveryMultiplier: delivery.multiplier, deliverySurcharge,
    subtotal, total, deposit, balance,
    carePlan: care?.id ?? null, carePlanMonthly: care?.monthlyPrice ?? 0,
    timelineWeeks, currency: 'GBP' };
}
export function gbp(n: number): string {
  return '£' + n.toLocaleString('en-GB', { minimumFractionDigits: n % 1 === 0 ? 0 : 2, maximumFractionDigits: 2 });
}
