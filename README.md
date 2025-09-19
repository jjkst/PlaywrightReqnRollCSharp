## Playwright offical documentation

https://playwright.dev/


## Setup Playwright

```bash
dotnet build
```

Install required browsers.

```bash
bin/Debug/net8.0/playwright.ps1 install
```

## Running Codegen and recording a test

```bash
bin/Debug/net8.0/playwright.ps1 codegen
```

more details find here https://playwright.dev/dotnet/docs/codegen-intro

## Running tests

```bash
dotnet test --settings app.runsettings
```