# QuickLook ICS Agenda v0.1.0

First public release of QuickLook ICS Agenda.

## Features

- Agenda-style preview for `.ics` / iCalendar files
- Multiple events grouped by day
- All-day events
- Start and end times
- Location, organizer, and description
- Human-readable recurrence summaries
- `FREQ`, `INTERVAL`, `COUNT`, `BYDAY`, and `UNTIL` recurrence information
- UTC timestamp conversion
- RFC 5545 folded-line handling

Recurring events are intentionally shown once rather than expanded into multiple occurrences, keeping the QuickLook preview compact.

## Known limitations

- `TZID` / `VTIMEZONE` definitions are not fully resolved yet.
- Floating/TZID timestamps are displayed using their wall-clock value.
- This is a previewer, not a complete RFC 5545 calendar implementation.

## Installation

Download `QuickLook.Plugin.IcsAgenda.qlplugin`, open its QuickLook preview with **Space**, choose **Install**, and restart QuickLook.
