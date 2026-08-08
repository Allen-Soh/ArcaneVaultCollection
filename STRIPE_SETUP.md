# Stripe Setup Guide

How the ArcaneVault cosmetics shop's Stripe integration is wired up, and what
you need to configure to run it yourself (locally or in production).

## How it works

- The Buy button on a Shop card calls `POST api/Payment/checkout`
  (`ArcaneVaultAPI/Controllers/PaymentController.cs`), which creates a Stripe
  **Checkout Session** and returns its hosted URL. The browser redirects
  there directly — there's no custom card-entry form in this app.
- After the customer pays (or cancels), Stripe redirects back to
  `/PaymentPages/Success` or `/PaymentPages/Failed` in the Razor Pages app.
- Separately, Stripe calls `POST api/Payment/webhook` server-to-server. This
  webhook is the **only** place a `Payment` row is ever marked `Success` —
  the success-page redirect never does this itself, so a user can't fake a
  purchase by hitting the success URL directly.

## 1. Get your API keys

1. Sign up / log in at [dashboard.stripe.com](https://dashboard.stripe.com).
2. Make sure you're in **Test mode** (toggle top-right) while developing.
3. Go to **Developers → API keys**.
4. Copy the **Publishable key** (`pk_test_...`) and **Secret key**
   (`sk_test_...`).
5. Put them in `ArcaneVaultAPI/appsettings.json`:
   ```json
   "Stripe": {
     "PublishableKey": "pk_test_...",
     "SecretKey": "sk_test_...",
     "WebhookSecret": "whsec_..."
   }
   ```
   (The publishable key isn't currently used server-side — it's only needed
   if a future phase adds Stripe.js/Elements to the frontend. The secret key
   is what `PaymentController` uses to create Checkout Sessions.)

## 2. Register the webhook endpoint

**Production**: Developers → Webhooks → **Add endpoint**.
- Endpoint URL: `https://<your-domain>/api/Payment/webhook`
- Events to send: `checkout.session.completed`
- After creating it, copy the **Signing secret** (`whsec_...`) into
  `Stripe:WebhookSecret` in `appsettings.json`.

**Local development**: Stripe can't reach `https://localhost:7040` from the
internet, so use the [Stripe CLI](https://stripe.com/docs/stripe-cli) instead:

```bash
stripe login
stripe listen --forward-to https://localhost:7040/api/Payment/webhook
```

`stripe listen` prints a webhook signing secret (`whsec_...`) — use **that**
one in `Stripe:WebhookSecret` while developing locally; it's different from
the dashboard's production endpoint secret. Trigger a test event with:

```bash
stripe trigger checkout.session.completed
```

## 3. Test-mode vs live-mode

- Test-mode keys (`pk_test_...`/`sk_test_...`) only work with
  [Stripe's test card numbers](https://stripe.com/docs/testing) (e.g.
  `4242 4242 4242 4242`, any future expiry, any CVC) — no real money moves.
- Switching the dashboard toggle to **Live mode** gives you a separate set of
  `pk_live_...`/`sk_live_...` keys and a separate webhook endpoint (with its
  own signing secret) that must be registered again under Live mode. Don't
  reuse test-mode keys in production or vice versa — they're tied to
  completely separate data.
- Go live only once you've completed Stripe's account activation
  (business details, bank account) in the dashboard.

## 4. Required NuGet package

Already referenced in `ArcaneVaultAPI/ArcaneVaultAPI.csproj`:

```xml
<PackageReference Include="Stripe.net" Version="52.2.0" />
```

No further installation needed — `dotnet restore` picks it up.

## 5. What's already wired up vs. what needs your keys

| Piece | Status |
|---|---|
| `Stripe.net` package reference | ✅ Already in the `.csproj` |
| `StripeConfiguration.ApiKey` set at startup | ✅ Already in `Program.cs` |
| Checkout Session creation (`PaymentController.CreateCheckout`) | ✅ Built |
| Webhook signature verification + fulfillment | ✅ Built |
| Payment receipt email | ✅ Built |
| Your actual Stripe test/live keys in `appsettings.json` | ⚠️ Replace the placeholder values with your own before testing |
