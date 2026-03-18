# Playwright ReqnRoll C# (.NET) Framework

A BDD test automation framework using **Playwright** with **C# (.NET 9)** and **Reqnroll** (Gherkin/Cucumber), featuring ExtentReports HTML reporting, NLog logging, and parallel execution via MSTest.

---

## Table of Contents

- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Tech Stack](#tech-stack)
- [Features](#features)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Configuration](#configuration)
- [Running Tests](#running-tests)
- [Reporting](#reporting)
- [Logging](#logging)
- [Using Playwright Agents under Claude](#using-playwright-agents-under-claude)
  - [Test Planner Agent](#1-test-planner-agent)
  - [Test Generator Agent](#2-test-generator-agent)
  - [Test Healer Agent](#3-test-healer-agent)
  - [MCP Server Setup](#mcp-server-setup)

---

## Architecture

The framework follows a layered architecture combining BDD with the Page Object Model:

```
Feature Files (Gherkin)
        |
   Step Definitions (Reqnroll Bindings)
        |
   Page Objects (BasePage -> LoginPage, ProductsPage, CartPage, CheckoutPage)
        |
   Playwright API (Browser Automation)
        |
   Browser (Chrome / Edge / Chromium / WebKit)
```

**Key Design Decisions:**

- **Reqnroll** (successor to SpecFlow) provides BDD with Gherkin syntax, integrated with MSTest for parallel execution.
- **Page Object Model (POM)** with `BasePage` providing shared utilities (dialog handling, element queries, API capture, table extraction) inherited by feature-specific pages.
- **Dependency Injection** via Reqnroll's BoDi container — browser, context, page, and `PlaywrightContext` are registered per-scenario and injected into step definitions.
- **Thread-safe user account pooling** via `UserAccountManager` with `ConcurrentQueue` — enables parallel test execution with unique user accounts per feature.
- **Hooks-driven lifecycle** — `BeforeTestRun` / `BeforeFeature` / `BeforeScenario` / `AfterStep` / `AfterScenario` / `AfterTestRun` manage browser setup, reporting, tracing, and teardown.
- **ExtentReports** generates rich HTML reports with Gherkin step nodes, screenshots on failure, and trace files.
- **NLog** provides structured logging to both console and file.

---

## Project Structure

```
PlaywrightReqnRollCSharp/
├── .claude/
│   └── agents/
│       ├── playwright-test-generator.md   # AI agent: generates tests from plans
│       ├── playwright-test-healer.md      # AI agent: debugs & fixes failing tests
│       └── playwright-test-planner.md     # AI agent: creates comprehensive test plans
├── Features/
│   ├── LoginLogout.feature              # Login/Logout scenarios
│   └── Saucedemo.feature                # E-commerce scenarios (products, cart, checkout)
├── PageObjects/
│   ├── BasePage.cs                      # Base page with shared utilities
│   ├── LoginPage.cs                     # Login page interactions
│   ├── ProductsPage.cs                  # Product listing and cart actions
│   ├── CartPage.cs                      # Cart operations
│   └── CheckoutPage.cs                  # Checkout flow
├── Steps/
│   ├── Hooks.cs                         # Reqnroll hooks (lifecycle, browser, reporting)
│   ├── LoginLogoutSteps.cs              # Login/Logout step definitions
│   └── SaucedemoSteps.cs                # Saucedemo step definitions
├── Support/
│   ├── PlaywrightContext.cs             # Page reference wrapper with DI
│   ├── EnvironmentVariables.cs          # Environment-based URL/DB config
│   ├── Functions.cs                     # Utility functions (DataTable, random values, SQL)
│   ├── TestConstants.cs                 # Timeout constants
│   ├── NLogTestConfig.cs                # NLog configuration
│   ├── UserAccountManager.cs            # Thread-safe user account pool
│   └── ReqnrollCustomStepAttribute.cs   # Custom [Prepare] step attribute
├── Logs/                                # NLog output (git-ignored)
├── TestResults/                         # Reports, screenshots, traces (git-ignored)
├── .mcp.json                            # MCP server config for Playwright
├── app.runsettings                      # MSTest run settings
├── PlaywrightReqnRollCSharp.csproj      # Project file
└── README.md
```

---

## Tech Stack

| Component           | Technology                                |
|---------------------|-------------------------------------------|
| Framework           | .NET 9.0                                  |
| Test Runner         | MSTest v4.0.2                             |
| BDD Framework       | Reqnroll.MsTest v3.2.1                    |
| Browser Automation  | Microsoft.Playwright.MSTest v1.56.0       |
| Reporting           | ExtentReports v5.0.4                      |
| Logging             | NLog v6.0.6                               |
| Language            | C# (latest)                               |

---

## Features

- **BDD with Gherkin** — Write tests in plain English using Given/When/Then syntax via Reqnroll
- **Parallel Execution** — 3 workers at ClassLevel scope via MSTest
- **Multi-Browser Support** — Chrome, Microsoft Edge, Chromium, WebKit
- **Headless/Headed Mode** — Toggle via run settings
- **Page Object Model** — Layered page abstractions with `BasePage` utilities
- **Dependency Injection** — Reqnroll BoDi container auto-injects browser, page, and context
- **Thread-Safe Account Pool** — `UserAccountManager` assigns unique users per parallel feature
- **ExtentReports HTML** — Rich reports with Gherkin steps, screenshots on failure, and traces
- **Playwright Tracing** — Screenshots, snapshots, and source captured per scenario
- **Structured Logging** — NLog to console and daily rotating log files
- **Data-Driven Testing** — Reqnroll DataTables for parameterized scenarios
- **Dialog Handling** — Accept/dismiss browser dialogs with message capture
- **HTML Table Extraction** — Parse HTML tables into `System.Data.DataTable`

### Test Scenarios Covered

| Feature       | Scenario                    | Description                                      |
|---------------|-----------------------------|--------------------------------------------------|
| Login/Logout  | Login and Logout            | Login, open menu, logout, validate logged out     |
| Saucedemo     | Check all products exist    | Validate all 6 product names on inventory page    |
| Saucedemo     | Buy products                | Add to cart, checkout, complete order              |
| Saucedemo     | Remove products             | Add to cart, remove item, validate removal         |

---

## Prerequisites

- **.NET SDK** >= 9.0
- **PowerShell** (for Playwright browser installation)

---

## Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd PlaywrightReqnRollCSharp
   ```

2. **Build the project** (restores NuGet packages)
   ```bash
   dotnet build
   ```

3. **Install Playwright browsers**
   ```powershell
   pwsh bin/Debug/net9.0/playwright.ps1 install
   ```

---

## Configuration

### Run Settings (`app.runsettings`)

| Parameter         | Values                                     | Description                          |
|-------------------|--------------------------------------------|--------------------------------------|
| `Environment`     | `Test` / `Prod`                            | Target environment                   |
| `Browser`         | `Chrome` / `MSEdge` / `Chromium` / `WebKit`| Browser to run tests in              |
| `HeadlessBrowser`  | `true` / `false`                          | Run browser in headless mode         |
| `Users`           | comma-separated list                       | Test user accounts for parallel pool |
| `Password`        | string                                     | Shared password for test users       |

### MSTest Parallelization

```xml
<Parallelize>
    <Workers>3</Workers>
    <Scope>ClassLevel</Scope>
</Parallelize>
```

Each feature runs in its own class with a unique user account from the pool.

### Environment URLs (`EnvironmentVariables.cs`)

| Environment | App URL                       | API URL                         | DB Server       |
|-------------|-------------------------------|---------------------------------|-----------------|
| Test        | https://www.saucedemo.com/    | https://api.saucedemo.com/      | localhost-test  |
| Prod        | https://www.saucedemo.com/    | https://api.saucedemo.com/      | localhost-prod  |

---

## Running Tests

### Run all tests
```bash
dotnet test --settings app.runsettings
```

### Run tests in headed mode (visible browser)
Edit `app.runsettings` and set `HeadlessBrowser` to `false`, or override inline:
```bash
dotnet test --settings app.runsettings -- TestRunParameters.Parameter(name=\"HeadlessBrowser\",value=\"false\")
```

### Run a specific feature by filter
```bash
dotnet test --settings app.runsettings --filter "FullyQualifiedName~LoginLogout"
```

### Run with a different browser
```bash
dotnet test --settings app.runsettings -- TestRunParameters.Parameter(name=\"Browser\",value=\"MSEdge\")
```

### Generate test code with Playwright Codegen
```powershell
pwsh bin/Debug/net9.0/playwright.ps1 codegen https://www.saucedemo.com
```

More details: https://playwright.dev/dotnet/docs/codegen-intro

---

## Reporting

### ExtentReports HTML

After a test run, an HTML report is generated at:

```
TestResults/PlaywrightReqnRollCSharp.test-run-<date>.html
```

The report includes:
- **Feature-level grouping** — each feature file becomes a top-level node
- **Scenario nodes** — each scenario nested under its feature
- **Gherkin step details** — Given/When/Then/And/But steps with pass/fail status
- **Screenshots on failure** — full-page screenshots embedded as base64 images
- **Playwright traces on failure** — `.zip` trace files saved in `TestResults/`

### Viewing Playwright Traces

On failure, trace files are saved as `TestResults/<ScenarioName>.zip`. Open them with:

```bash
npx playwright show-trace TestResults/<ScenarioName>.zip
```

### Report Artifacts

| Artifact             | Location                           | Description                            |
|----------------------|------------------------------------|----------------------------------------|
| HTML Report          | `TestResults/*.html`               | ExtentReports dashboard                |
| Screenshots          | `TestResults/*.png`                | Full-page screenshots on failure       |
| Trace Files          | `TestResults/*.zip`                | Playwright traces (snapshots, network) |
| Log Files            | `Logs/test-run-<date>.log`         | NLog structured log output             |

All `TestResults/` and `Logs/` directories are git-ignored.

---

## Logging

NLog writes to both console and file:

- **Console** — `[timestamp] [level] [logger] message`
- **File** — `Logs/test-run-<date>.log` (daily rolling)

Log levels: Info through Fatal. Scenario context, page errors, and console errors are logged after each scenario.

---

## Using Playwright Agents under Claude

This project includes three AI-powered agents that integrate with Claude Code via the Playwright MCP (Model Context Protocol) server. These agents can plan, generate, and fix tests autonomously by controlling a real browser.

### MCP Server Setup

The `.mcp.json` file configures the Playwright MCP server:

```json
{
  "mcpServers": {
    "playwright-test": {
      "command": "npx",
      "args": ["playwright", "run-test-mcp-server"]
    }
  }
}
```

This server provides Claude with browser automation tools (click, type, navigate, snapshot, etc.) and test management tools (run, debug, list, generate). The MCP server uses `npx` so Node.js must be available on the PATH.

---

### 1. Test Planner Agent

**Purpose:** Creates comprehensive test plans by exploring a live web application.

**When to use:** When you need to design test coverage for a new feature or application before writing any test code.

**How to invoke in Claude Code:**
```
@playwright-test-planner Create a test plan for https://www.saucedemo.com
```

**What it does:**
1. Opens the application in a real browser via `planner_setup_page`
2. Explores the interface by navigating pages, clicking elements, and taking snapshots
3. Maps out user flows and identifies critical paths
4. Designs test scenarios covering happy paths, edge cases, and error handling
5. Saves a structured markdown test plan via `planner_save_plan`

**Output:** A markdown file with numbered test scenarios, step-by-step instructions, expected outcomes, and success/failure criteria.

---

### 2. Test Generator Agent

**Purpose:** Converts test plans into working Playwright test code (Gherkin features + C# step definitions) by executing each step in a real browser.

**When to use:** After you have a test plan and need to create automated test scripts.

**How to invoke in Claude Code:**
```
@playwright-test-generator Generate tests from the test plan in specs/plan.md
```

**What it does:**
1. Reads the test plan with all steps and verification criteria
2. Sets up the browser page via `generator_setup_page`
3. Manually executes each step in the browser to validate it works
4. Reads the execution log via `generator_read_log`
5. Generates both a `.feature` file and `Steps.cs` file following project conventions:
   - Feature files in `Features/` with Gherkin syntax
   - Step definitions in `Steps/` with Reqnroll bindings
   - Page objects in `PageObjects/` if new pages are needed
   - Uses existing Page Object Model and DI patterns

**Output:** Gherkin `.feature` files and C# step definition classes ready to run.

---

### 3. Test Healer Agent

**Purpose:** Automatically debugs and fixes failing Playwright tests.

**When to use:** When tests are failing and you need automated diagnosis and repair.

**How to invoke in Claude Code:**
```
@playwright-test-healer Fix the failing tests
```

**What it does:**
1. **Runs all tests** via `test_run` to identify failures
2. **Debugs each failure** via `test_debug` (pauses at the error)
3. **Investigates** using MCP tools:
   - Takes page snapshots to see current state
   - Checks console messages for errors
   - Inspects network requests
   - Generates new locators via `browser_generate_locator`
4. **Analyzes root cause**: stale selectors, timing issues, data dependencies, app changes
5. **Fixes the code**: updates Page Object locators, assertions, or step logic
6. **Re-runs** to verify the fix
7. **Iterates** until all tests pass (or marks unfixable tests with `[Ignore]`)

**Key principles:**
- Fixes one error at a time and retests
- Updates locators in Page Object classes, not in step definitions
- Prefers robust solutions over quick hacks
- Never uses deprecated Playwright APIs

---

### Agent Workflow Summary

```
     Test Planner                Test Generator              Test Healer
     ───────────                ───────────────             ────────────
    Explore app UI          Read test plan               Run tests
         │                       │                            │
    Map user flows          Execute steps in browser     Identify failures
         │                       │                            │
    Design scenarios        Read execution log           Debug with MCP tools
         │                       │                            │
    Save test plan ──────> Generate .feature +           Fix & verify
    (markdown)              Steps.cs + PageObjects       (iterate until green)
```

### Tips for Using Agents

- **Start with the Planner** to create a test plan, then pass it to the Generator
- **Run the Healer** after application changes break existing tests
- Agents use the **Sonnet model** for cost-effective automation
- The MCP server launches a **real browser** — agents interact with the actual application
- Agent configurations live in `.claude/agents/` and can be customized
