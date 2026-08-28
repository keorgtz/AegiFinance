import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests/e2e",
  timeout: 45_000,
  retries: 1,
  use: { baseURL: process.env.E2E_BASE_URL ?? "http://127.0.0.1:3001", trace: "retain-on-failure", screenshot: "only-on-failure" },
  projects: [
    { name: "desktop-chrome", use: { ...devices["Desktop Chrome"] } },
    { name: "mobile-chrome", use: { ...devices["Pixel 7"] } }
  ]
});
