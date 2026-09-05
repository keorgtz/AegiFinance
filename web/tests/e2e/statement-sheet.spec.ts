import { expect, test } from "@playwright/test";
import manifest from "../../generated/ui-control-manifest.json";

for (const theme of ["light", "dark"]) for (const width of [390, 768, 1440]) {
  test(`statement keyboard capture ${theme} ${width}`, async ({ page }) => {
    await page.setViewportSize({ width, height: 950 });
    await page.addInitScript(theme => localStorage.setItem("aegifinance-theme", theme), theme);
    const permissions = [...new Set(manifest.map(item => item.requiredPermissionCode).filter(Boolean))];
    let uploaded = "";
    const batch = { id: "batch", status: "Preview", validRecords: 1, duplicateRecords: 0, incompleteRecords: 0, rejectedRecords: 0, recordsImported: 0, rows: [{ id: "row", rowNumber: 2, description: "Pago septiembre", amount: 500, issues: [] }] };
    await page.route("**/api/**", async route => {
      const path = new URL(route.request().url()).pathname;
      let data: unknown = [];
      if (["/ledger", "/clients", "/services", "/subscriptions"].some(endpoint => path.endsWith(endpoint))) data = { items: [], totalCount: 0, totalPages: 0 };
      if (path.endsWith("/auth/refresh")) data = { accessToken: "test", user: { id: "user", userName: "tester", fullName: "Prueba", permissions, roles: [], userType: "Administrator", uiPolicies: {} } };
      if (path.endsWith("/bankaccounts")) data = { items: [{ id: "account", name: "Cuenta principal", currency: "MXN", isActive: true, ledgerBalance: 0 }], totalCount: 1 };
      if (path.endsWith("/bank-imports/preview")) { uploaded = route.request().postData() ?? ""; data = batch; }
      if (path.endsWith("/bank-imports/batch/confirm")) data = { ...batch, status: "Committed", recordsImported: 1 };
      await route.fulfill({ json: data });
    });
    await page.goto("/ledger");
    await expect(page.getByRole("heading", { name: "1. Captura tu estado de cuenta" })).toBeVisible();
    await expect(page.locator("html")).toHaveAttribute("data-theme", theme);
    const date = page.locator('[data-ui-control="ledger.sheet.field.date"]').first();
    await date.fill("2026-09-01");
    await date.press("Enter");
    const concept = page.locator('[data-ui-control="ledger.sheet.field.description"]').first();
    await expect(concept).toBeFocused();
    await concept.fill('Pago "septiembre", cliente');
    await concept.press("Enter");
    await page.locator('[data-ui-control="ledger.sheet.field.debit"]').first().press("Enter");
    const credit = page.locator('[data-ui-control="ledger.sheet.field.credit"]').first();
    await credit.fill("500");
    await credit.press("Enter");
    const balance = page.locator('[data-ui-control="ledger.sheet.field.balance"]').first();
    await balance.fill("1500");
    await balance.press("Enter");
    await expect(page.locator('[data-ui-control="ledger.sheet.field.date"]').nth(1)).toBeFocused();
    await page.getByRole("button", { name: "Eliminar movimiento 2", exact: true }).click();
    await expect(page.locator('[data-ui-control="ledger.sheet.field.date"]')).toHaveCount(1);
    await page.locator('[data-ui-control="ledger.bankImports.tabs.imports"]').click();
    await page.locator('[data-ui-control="ledger.sheet.tab"]').click();
    await expect(concept).toHaveValue('Pago "septiembre", cliente');
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
    await page.locator('[data-ui-control="ledger.sheet.account"]').getByRole("combobox").click();
    await page.getByRole("option", { name: "Cuenta principal · MXN" }).click();
    await page.locator('[data-ui-control="ledger.sheet.preview"]').click();
    await expect(page.getByText("1 válidos · 0 duplicados · 0 por corregir")).toBeVisible();
    expect(uploaded).toContain('"Pago ""septiembre"", cliente"');
    expect(uploaded).toContain('"2026-09-01"');
    await page.locator('[data-ui-control="ledger.sheet.confirm"]').click();
    await expect(page.getByText("1 movimientos guardados")).toBeVisible();
    await page.screenshot({ path: `test-results/sheet-${theme}-${width}.png`, fullPage: true });
  });
}
