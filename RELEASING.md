# Releasing QuickLook ICS Agenda

This file documents the maintainer workflow for publishing a new version of QuickLook ICS Agenda.

## Versioning

Two version numbers are used:

- **GitHub release / Git tag:** semantic versioning, for example `v0.1.0`, `v0.1.1`, or `v0.2.0`.
- **QuickLook plugin version:** the positive integer in `QuickLook.Plugin.Metadata.config`.

For the first public release:

```text
GitHub release: v0.1.0
QuickLook plugin version: 1
```

For every subsequently published `.qlplugin`, increment the QuickLook plugin version. Do not reuse a previously published internal version number.

## 1. Update the QuickLook plugin version

Edit:

```text
QuickLook.Plugin.Metadata.config
```

Example:

```xml
<Version>1</Version>
```

The packaging script rejects missing, invalid, or zero versions.

## 2. Build

Make sure a compatible `QuickLook.Common.dll` is available locally in:

```text
lib\QuickLook.Common.dll
```

Then run:

```powershell
dotnet clean
dotnet restore
dotnet build -c Release
```

The build must finish without errors.

## 3. Create the plugin package

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\Scripts\pack-zip.ps1
```

This creates:

```text
QuickLook.Plugin.IcsAgenda.qlplugin
```

The `.qlplugin` file is ignored by Git and should be uploaded as a GitHub Release asset.

## 4. Test the package

Before publishing:

1. Completely exit QuickLook.
2. Remove an older development installation of the plugin if necessary.
3. Start QuickLook.
4. Select `QuickLook.Plugin.IcsAgenda.qlplugin` and press **Space**.
5. Install the plugin.
6. Restart QuickLook.
7. Preview `sample.ics`.
8. Test at least one real-world `.ics` file.
9. Verify normal and all-day events.
10. Verify a recurring event displays its recurrence summary correctly.

Do not publish the release if the package cannot be installed or the preview fails.

## 5. Commit the release state

Review the working tree:

```powershell
git status
git diff
```

Commit the release-ready source:

```powershell
git add .
git commit -m "Prepare v0.1.0 release"
```

Adjust the version in the commit message for later releases.

## 6. Create the Git tag

For `v0.1.0`:

```powershell
git tag -a v0.1.0 -m "First public release"
```

Push the branch and tag:

```powershell
git push origin main
git push origin v0.1.0
```

## 7. Create the GitHub Release

Create a GitHub Release from the matching tag.

For the first release:

```text
Tag: v0.1.0
Title: QuickLook ICS Agenda v0.1.0
```

Attach:

```text
QuickLook.Plugin.IcsAgenda.qlplugin
```

Use `RELEASE_NOTES_v0.1.0.md` as the basis for the first release notes. For later releases, write release notes describing only the changes in that release.

## 8. Final check

After publishing:

- Verify the GitHub Release is visible.
- Verify the `.qlplugin` asset can be downloaded.
- Install the downloaded release asset once to ensure the published package is the expected build.
