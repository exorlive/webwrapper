# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with
code in this repository.

## What this project is

**ExorLive WebWrapper** is a WPF desktop application that wraps the ExorLive SPA
(hosted at `https://auth.exorlive.com/signin/`) in a controllable Windows
desktop client. It enforces single-instance behavior via named pipes and
supports deep integration with healthcare systems (Procapita, Promed, Exor3)
through a pluggable `ProtocolProvider` architecture.

## Building

Open `ExorLive.Client.WebWrapper.sln` in Visual Studio. Build configurations map
to deployment targets:

| Configuration   | Target                   |
| --------------- | ------------------------ |
| Debug / Release | Standard ExorLive        |
| Procapita       | Procapita integration    |
| Goteborg        | Göteborg municipality    |
| Boras           | Borås municipality       |
| Uppsala         | Uppsala                  |
| VGR             | Västra Götalandsregionen |

Each configuration has a corresponding `App.<Config>.config` transform applied
by SlowCheetah.

Building requires:

- WiX Toolset (for SetupProject installer)
- A code signing certificate in the current user's personal certificate store

## Publishing checklist

1. Bump `AssemblyVersion` in
   `ExorLive.Client.WebWrapper/Properties/AssemblyInfo.cs`
2. Bump `Version` in `SetupProject/Product.wxs`
3. Add changelog entry in `Webpage/Default.aspx`
4. Publish `ExorLive.Client.WebWrapper` (Publish tab in VS) — builds Release,
   Procapita, Goteborg, Boras, VGR
5. Publish `Webpage` to `exorlivewebwrapper.scm.azurewebsites.net`

## Tests

PowerShell-based functional tests live in `FunctionalityTesting/`. Run via
`FunctionalityTesting/RunCmdTest.bat` or the script directly:
`FunctionalityTesting/CmdTesting.ps1`.

## Architecture

### Browser abstraction

`ExorLive.WebWrapper.Interface` defines `IBrowser` — the contract all browser
backends implement. The active backend is selected via `BrowserEngine` in
`App.config` (default: `EoWebBrowser`).

Three backends exist:

- **`WebWrapper.EoBrowser`** — Edge/Chromium (default)
- **`WebWrapper.WebViewBrowser`** — WebView2
- **`WebWrapper.WindowsBrowser`** — Legacy IE

### Single-instance & IPC

`ExorLive.Client.WebWrapper.App` implements `ISingleInstanceApp`. When a second
instance launches it forwards its arguments via a named pipe
(`exorlivepipe.<PID>`) to the running instance and exits. The pipe server is in
`ExorLive.Client.WebWrapper.NamedPipe/NPServer.cs`.

`NamedPipeTestServer/` and `ExorLiveNamedPipeSampleClient/` are standalone
utilities for testing the IPC channel.

### Desktop integration (VB.NET)

`ExorLive.Desktop` is a VB.NET library that implements the `ProtocolProvider`
interface. It handles integration with external healthcare systems. The active
provider is specified by the `ProtocolProvider` app setting (fully qualified
type name + assembly). Available providers:

- `ExorLive.Desktop.Arguments.Component` — command-line argument protocol
  (default)
- `ExorLive.Desktop.Exor3.Component` — Exor3 integration
- `ExorLive.Desktop.Procapita.Component` — Procapita integration
- `ExorLive.Desktop.Promed.Component` — Promed integration

### Configuration transforms

`SlowCheetah` applies `App.<Configuration>.config` transforms at build time.
Each named configuration overrides `AppUrl`, `AdfsUrl`, `DistributorName`,
`ProtocolProvider`, and related settings.

### Webpage (ASP.NET)

The `Webpage` project is a deployment/documentation site hosted at
`https://webwrapper.exorlive.com`. It contains the changelog and serves version
metadata for the auto-update check.
