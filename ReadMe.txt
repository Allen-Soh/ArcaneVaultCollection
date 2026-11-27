================================================================
 ARCANE VAULT COLLECTION
================================================================

The application is made of two ASP.NET Core (.NET 10) projects that run together:

  ArcaneVaultAPI  - Web API + SQLite database (ArcaneVault.db)   https://localhost:7040
  ArcaneVaultRP   - Razor Pages website (the part you open)      https://localhost:7051

The website calls the API for all of its data, so BOTH must be running.


----------------------------------------------------------------
 LOGIN CREDENTIALS
----------------------------------------------------------------

  Role          Username   Password    Email
  ------------  ---------  ----------  ------------------
  Admin/Staff   Admin      Admin123    admin@example.com
  Admin/Staff   Admin1     Admin123    admin1@example.com
  User          Allen      Password123 allen@example.com

Usernames and passwords are case-sensitive.
The emails are placeholders, so "Forgot password" codes for these accounts are not
delivered anywhere. Register a new account with a real email to test email features.


----------------------------------------------------------------
 PREREQUISITES
----------------------------------------------------------------

  - .NET 10 SDK                https://dotnet.microsoft.com/download
  - Visual Studio 2022/2026 (with "ASP.NET and web development"), OR any terminal
  - Trusted HTTPS dev certificate (one-time):
        dotnet dev-certs https --trust
  - Optional: ngrok (only needed to test the Stripe payment webhook, see below)


----------------------------------------------------------------
 STEP 1 - CREATE THE CONFIG FILES
----------------------------------------------------------------

appsettings.json is git-ignored (it holds API keys), so a fresh clone will NOT have it.
Create these two files and fill in your own keys.

ArcaneVaultAPI/appsettings.json
-------------------------------
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "RpSettings": { "BaseUrl": "https://localhost:7051" },
  "Jwt": {
    "Key": "<any random string, at least 32 characters>",
    "Issuer": "https://localhost:7040",
    "Audience": "https://localhost:7051"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": "587",
    "Username": "<gmail address>",
    "Password": "<gmail app password>",
    "From": "<gmail address>"
  },
  "Cloudinary": { "CloudName": "<cloud name>", "ApiKey": "<api key>", "ApiSecret": "<api secret>" },
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}

ArcaneVaultRP/appsettings.json
------------------------------
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ApiSettings": { "BaseUrl": "https://localhost:7040/api" },
  "ReCaptcha": { "SiteKey": "<recaptcha site key>", "SecretKey": "<recaptcha secret key>" }
}

Notes:
  - Without Smtp, emails (verification, password reset, report notices) will fail.
  - Without Cloudinary, image uploads will fail. Existing seeded images still display.
  - Without Stripe, the cosmetics shop checkout will fail. See STRIPE_SETUP.md.
  - Without reCAPTCHA keys, the captcha on the Registration page will fail.


----------------------------------------------------------------
 STEP 2 - RUN THE APPLICATION
----------------------------------------------------------------

Option A - Visual Studio
  1. Open ArcaneVaultCollection.slnx.
  2. Right-click the Solution > "Configure Startup Projects..." > "Multiple startup projects".
     Set both ArcaneVaultAPI and ArcaneVaultRP to "Start". Put ArcaneVaultAPI first.
     (ArcaneVaultCollection.slnLaunch.user already contains this profile, so it may be set already.)
  3. Choose the "https" launch profile and press F5.
  4. Open https://localhost:7051 and log in with one of the accounts above.

Option B - Terminal (two windows, from the ArcaneVaultCollection folder)
  Window 1 (API - start this first):
      cd ArcaneVaultAPI
      dotnet run --launch-profile https

  Window 2 (website):
      cd ArcaneVaultRP
      dotnet run --launch-profile https

  Wait for both windows to show "Application started", then open https://localhost:7051.
  Press Ctrl+C in each window to stop.

The database (ArcaneVaultAPI/ArcaneVault.db) is included. If it is deleted, the API
recreates it and re-seeds the default accounts and sample data on the next start.


----------------------------------------------------------------
 STRIPE PAYMENTS (OPTIONAL, LOCAL TESTING)
----------------------------------------------------------------

Stripe has to reach the API's webhook, so expose the API with ngrok in a third terminal:

    ngrok http https://localhost:7040

Use the https ngrok URL + /api/Payment/webhook as the webhook endpoint in Stripe.
Full details are in STRIPE_SETUP.md.


----------------------------------------------------------------
 DEPLOYING (RENDER, FREE TIER)
----------------------------------------------------------------

Each project has a Dockerfile, and render.yaml at the repo root describes both services.
In Render: New + > Blueprint > select this repo, then enter the settings it asks for.
These are the same keys as appsettings.json, with "__" in place of ":" (for example Jwt__Key).

Free-tier limits:
  - There is no persistent disk. Data created online resets to the committed
    ArcaneVault.db whenever a service restarts, sleeps or redeploys.
  - Services sleep after about 15 minutes idle. The first visit afterwards takes about a minute.
