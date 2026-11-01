# GiftSync status

Last updated: 1 Nov 2026. Current phase: **Phase 1**.

Back to [README](../README.md) · Design: [DESIGN.md](DESIGN.md)

## Done

- DESIGN.md v2 written, including evidence/assumptions, Stripe revision (ADR-7, ADR-8) and POST create-only (ADR-9).
- Dataverse tables, columns and alternate keys created; solution metadata cloned and unpacked to `power-platform/`.
- Entra app registration, application user and custom security role created; `WhoAmI` works.
- Idempotency verified at the API level (duplicate `POST` returns `412` / `0x80060892`).
- Solution skeleton present: `src/` (Domain, Application, Infrastructure) and `tests/` projects, `GiftSync.sln`, `Directory.Build.props`, `.gitignore`.

## Not done yet

- Stripe: first Payment Link test payment, `stripe listen`, captured fixture `fixtures/stripe/checkout.session.completed.json`.
- Confirmation of exact Stripe field names for donor email, amount and payment intent id.
- Contact read/create tests with the app user.
- Domain, Application and Infrastructure code and tests.
- Functions project (`WebhookReceiver`, `DonationProcessor`).
- Power Automate Flow 1, exported and unpacked.

## Phase 1 plan

1. Repo and solution skeleton.
2. Domain: `Money`, `EmailAddress`, `PaymentId`, `DonationEvent` (tests first).
3. Application: ports, `ProcessDonationEvent`, `ProcessOutcome` (tests against fakes).
4. Infrastructure `Dataverse/`: auth, typed `HttpClient`, supporter find/create, donation record, error classification (WireMock.Net contract tests).
5. Infrastructure `Stripe/`: signature verification and event mapper.
6. Functions: `WebhookReceiver` and `DonationProcessor`, run locally with Azurite.
7. Power Automate Flow 1 on row created in `hl_donation`.
9. **Done when:** resending the same Stripe event five times creates exactly one `hl_donation` row and one notification.

## Open items

- Put the Entra client secret expiry date in a calendar.
- `If-None-Match: *` returning `404 / 0x80060891` is unexplained; not used.

Phases 2-4 (SQL inbox, reconciliation, CI/CD, Bicep, donation page, recurring giving, optional plugin) are in DESIGN.md section 13.
