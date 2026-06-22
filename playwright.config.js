// @ts-check
const { defineConfig } = require("@playwright/test");
const path = require("node:path");

const port = Number(process.env.PLAYWRIGHT_PORT || 5307);
const baseURL = process.env.PLAYWRIGHT_BASE_URL || `http://127.0.0.1:${port}`;
const playwrightDbPath = path.join(__dirname, "HospitalManagementApp", "HospitalManagementApp.playwright.db");

module.exports = defineConfig({
  testDir: "./playwright-tests",
  timeout: 60_000,
  expect: {
    timeout: 10_000
  },
  fullyParallel: false,
  workers: 1,
  reporter: [["list"], ["html", { open: "never" }]],
  use: {
    baseURL,
    extraHTTPHeaders: {
      Accept: "application/json"
    }
  },
  webServer: {
    command: [
      "cp HospitalManagementApp/HospitalManagementApp.local.db HospitalManagementApp/HospitalManagementApp.playwright.db",
      [
        "ASPNETCORE_ENVIRONMENT=Development",
        `ConnectionStrings__DefaultConnection='Data Source=${playwrightDbPath}'`,
        `dotnet run --project HospitalManagementApp/HospitalManagementApp.csproj --urls ${baseURL}`
      ].join(" ")
    ].join(" && "),
    url: baseURL,
    reuseExistingServer: !process.env.CI,
    timeout: 120_000
  }
});
