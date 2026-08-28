import { expect, test } from "@playwright/test";

const userName = process.env.E2E_USERNAME;
const password = process.env.E2E_PASSWORD;

test.beforeEach(async ({ page }) => {
  test.skip(!userName || !password, "E2E_USERNAME and E2E_PASSWORD are required on the deployment host.");
  await page.goto("/login");
  await page.getByLabel("Usuario o correo").fill(userName!);
  await page.getByLabel("Contraseña").fill(password!);
  await page.getByRole("button", { name: "Iniciar sesión" }).click();
  await expect(page).toHaveURL(/\/dashboard/);
});

for (const flow of [
  { name: "charge", route: "/billing", heading: /Cargos|Facturación/i },
  { name: "bank import and reconciliation", route: "/ledger", heading: /Ledger|Movimientos|Conciliación/i },
  { name: "payment application", route: "/allocations", heading: /Aplicación|Asignación|Pagos/i },
  { name: "account statement", route: "/account-statement", heading: /Estado de cuenta/i }
]) {
  test(`${flow.name} workflow is reachable and error-free`, async ({ page }) => {
    await page.goto(flow.route);
    await expect(page.getByRole("heading", { name: flow.heading }).first()).toBeVisible();
    await expect(page.getByText(/No se pudo cargar|Error inesperado/i)).toHaveCount(0);
  });
}

test("offline mode never queues a financial write", async ({ page, context }) => {
  await page.goto("/billing");
  await context.setOffline(true);
  await expect(page.getByText(/Sin conexión/i).first()).toBeVisible();
  await context.setOffline(false);
});

test("charge creation opens the permission-aware form", async ({ page }) => {
  await page.goto("/billing");
  await page.locator('[data-ui-control="billing.header.manual-charge"]').click();
  await expect(page.getByRole("dialog").getByText(/Cargo manual/i).first()).toBeVisible();
  await page.locator('[data-ui-control="billing.manual.cancel"]').click();
});

test("bank import and reconciliation workspaces expose their guarded actions", async ({ page }) => {
  await page.goto("/ledger");
  await page.locator('[data-ui-control="ledger.bankImports.tabs.imports"]').click();
  await expect(page.locator('[data-ui-control="ledger.bankImports.create"]')).toBeVisible();
  await page.locator('[data-ui-control="ledger.bankImports.create"]').click();
  await expect(page.locator('[data-ui-control="ledger.bankImports.form.preview"]')).toBeVisible();
  await page.locator('[data-ui-control="ledger.bankImports.form.cancel"]').click();
  await page.locator('[data-ui-control="ledger.reconciliation.tabs.workspace"]').click();
  await expect(page.locator('[data-ui-control="ledger.reconciliation.run"]')).toBeVisible();
});

test("payment application and statement preserve their guarded controls", async ({ page }) => {
  await page.goto("/allocations");
  await expect(page.locator('[data-ui-control="payments.auto.open"]')).toBeVisible();
  await expect(page.locator('[data-ui-control="payments.manual.open"]')).toBeVisible();
  await page.goto("/account-statement");
  await expect(page.locator('[data-ui-control="statements.filters.client"]')).toBeVisible();
  await expect(page.locator('[data-ui-control="statements.filters.currency"]')).toBeVisible();
});
