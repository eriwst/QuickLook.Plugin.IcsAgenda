# QuickLook ICS Agenda

Preview `.ics` / iCalendar files in [QuickLook for Windows](https://github.com/QL-Win/QuickLook) as a clean, compact agenda.

> **AI-assisted development:** This project was developed with extensive assistance from **OpenAI's ChatGPT**. Significant parts of the source code and documentation were AI-generated. The repository owner provided the project idea, requirements, testing, feedback, and design/release decisions.

<!-- Add a screenshot here once available:
![QuickLook ICS Agenda preview](docs/screenshot.png)
-->

## Download & install

For most users, **you do not need to build anything**.

1. Open the latest GitHub Release.
2. Download **`QuickLook.Plugin.IcsAgenda.qlplugin`** from the release assets.
3. Make sure QuickLook is running.
4. Select the downloaded `.qlplugin` file in Windows Explorer and press **Space**.
5. Choose **Install**.
6. Restart QuickLook.
7. Select any `.ics` file and press **Space**.

> Do not download GitHub's automatically generated **Source code (zip)** unless you want to build the plugin yourself.

Once the repository URL is known, a direct **Latest Release** badge/link can be added here.

## Features

- Agenda-style preview for `.ics` / iCalendar files
- Multiple events sorted chronologically and grouped by day
- All-day events
- Start and end times
- Location, organizer, and description
- Human-readable recurrence summaries
- UTF-8 and RFC 5545 folded-line handling
- Local conversion for UTC (`Z`) timestamps

## Recurring events

Recurring events are intentionally displayed **once** rather than expanded into all future occurrences. QuickLook is meant to provide a fast preview, not act as a full calendar application.

The recurrence rule is summarized directly in the agenda, for example:

```text
↻ Monthly · 10 events
↻ Every 2 weeks · Mon, Wed
↻ Daily · until 31 Dec 2026
```

The preview understands `FREQ`, `INTERVAL`, `COUNT`, `BYDAY`, and `UNTIL` for these summaries.

## Known limitations

- `TZID` / `VTIMEZONE` definitions are not fully resolved yet.
- Floating/TZID timestamps are currently displayed using their wall-clock value.
- Recurrence rules are summarized but deliberately not expanded into separate occurrences.
- The viewer is intended as a preview, not as a complete RFC 5545 calendar implementation.

## Build from source

### Requirements

You need:

- QuickLook for Windows
- .NET 8 SDK
- .NET Framework 4.6.2 Developer Pack
- A compatible `QuickLook.Common.dll` from your QuickLook installation

### Install the .NET SDK

Using `winget`:

```powershell
winget install Microsoft.DotNet.SDK.8
```

Close and reopen the terminal, then verify:

```powershell
dotnet --list-sdks
```

### Install the .NET Framework 4.6.2 Developer Pack

```powershell
winget install Microsoft.DotNet.Framework.DeveloperPack.4.6
```

If winget reports that the Developer Pack is already installed, no additional installation is normally necessary.

### NuGet

If restore fails because `Microsoft.NETFramework.ReferenceAssemblies` cannot be resolved, check the configured sources:

```powershell
dotnet nuget list source
```

If `nuget.org` is missing:

```powershell
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet restore
```

### QuickLook.Common.dll

The plugin needs `QuickLook.Common.dll` from a compatible QuickLook installation.

Create a local `lib` directory:

```powershell
mkdir lib
```

Copy the DLL to:

```text
lib\QuickLook.Common.dll
```

`QuickLook.Common.dll` is **not** distributed with this repository. Obtain a compatible copy from your local QuickLook installation.

If necessary, search for the DLL with PowerShell:

```powershell
Get-ChildItem "$env:LOCALAPPDATA","$env:APPDATA" -Filter QuickLook.Common.dll -Recurse -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty FullName
```

The project can alternatively reference QuickLook's `QuickLook.Common` source project at:

```text
QuickLook.Common\QuickLook.Common.csproj
```

### Plugin metadata

`QuickLook.Plugin.Metadata.config` contains the internal QuickLook plugin version. It must contain a positive integer, for example:

```xml
<Version>1</Version>
```

This internal version is separate from the GitHub/SemVer release number (`v0.1.0`).

## Build and package

From the project directory:

```powershell
dotnet clean
dotnet restore
dotnet build -c Release
powershell -ExecutionPolicy Bypass -File .\Scripts\pack-zip.ps1
```

The packaging script validates the metadata and Release output before creating:

```text
QuickLook.Plugin.IcsAgenda.qlplugin
```

The generated `.qlplugin` is intentionally ignored by Git. Attach it to a GitHub Release instead.

## License

This project is released under the MIT License. See `LICENSE.txt`.
