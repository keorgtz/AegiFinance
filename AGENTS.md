# AegiFinance — Project Rules

## Product invariants

- Ledger first: balances are derived from immutable, auditable movements. Never update a balance directly.
- A real Major Ledger must use balanced journal entries and lines; a bank transaction log is not a substitute.
- Keep one route and one view per module. Adapt content through permissions; never fork screens by role.
- Enforce tenant and subscription scope in the API and database. Hiding UI is not authorization.

## UI and accessibility

- Use the AegiPulse design system and semantic tokens. Every screen must work in light and dark themes.
- Recompose tasks for mobile; do not merely shrink desktop layouts.
- Provide loading, empty, partial, error, success, and offline states when applicable.
- Preserve WCAG 2.2 AA contrast, visible focus, keyboard operation, reduced motion, and 44 x 44 px touch targets.
- Keep one dominant primary action per context.

## Permission rule — mandatory

- Every new or modified interactive UI element (route, navigation item, button, link, menu action, tab, form control, and field) must have a stable semantic control key and a declarative permission policy.
- Use the centralized UI permission boundary and permission-aware primitives. Do not scatter ad-hoc role checks or inline permission logic through pages.
- If a modified control has no permission definition, add it to the generated catalog in the same change.
- The UI policy controls visibility, enabled/disabled state, and read-only state. The matching API operation must still enforce a business permission.
- New API authorization policies are synchronized automatically into the permission catalog at startup. Never add hard-coded role names to controllers or components.
- Security and accessibility escape controls such as logout and closing a modal must remain reachable; they still receive stable control keys but cannot be made unavailable.

## Verification

- A UI change is incomplete until type-check, production build, permission-catalog verification, and responsive checks at 390 px, 768 px, and 1440 px pass in both themes.
- A financial change is incomplete without tests for balance invariants, idempotency, authorization, tenant isolation, and audit evidence.

## Deployment rule — mandatory

- Every change must assess whether Dockerfiles, `docker-compose.yml`, environment variables, health checks, volumes, networks, startup order, or migration behavior need adjustment for a reproducible deployment.
- When deployment configuration is affected, update it in the same change. When no adjustment is needed, verification must still run `docker compose config` and record that the current Compose contract remains valid.
- Never finish a phase with undocumented manual deployment steps that should be encoded in Docker Compose or an image.
