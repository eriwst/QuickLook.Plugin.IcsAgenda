using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace QuickLook.Plugin.IcsAgenda
{
    internal static class IcsParser
    {
        public static CalendarDocument ParseFile(string path)
        {
            // RFC 5545 is UTF-8 by default. BOMs are handled by StreamReader.
            string text;
            using (var reader = new StreamReader(path, Encoding.UTF8, true))
                text = reader.ReadToEnd();

            return Parse(text);
        }

        public static CalendarDocument Parse(string text)
        {
            var doc = new CalendarDocument();
            CalendarEvent current = null;

            foreach (var line in Unfold(text))
            {
                if (line.Equals("BEGIN:VEVENT", StringComparison.OrdinalIgnoreCase))
                {
                    current = new CalendarEvent();
                    continue;
                }

                if (line.Equals("END:VEVENT", StringComparison.OrdinalIgnoreCase))
                {
                    if (current != null && current.Start != default(DateTime))
                    {
                        if (current.End == default(DateTime))
                            current.End = current.AllDay ? current.Start.AddDays(1) : current.Start;

                        doc.Events.Add(current);
                    }
                    current = null;
                    continue;
                }

                var colon = FindPropertyColon(line);
                if (colon < 0) continue;

                var left = line.Substring(0, colon);
                var rawValue = line.Substring(colon + 1);
                var semicolon = left.IndexOf(';');
                var name = (semicolon >= 0 ? left.Substring(0, semicolon) : left).ToUpperInvariant();
                var parameters = semicolon >= 0 ? left.Substring(semicolon + 1) : string.Empty;

                if (current == null)
                {
                    if (name == "X-WR-CALNAME")
                        doc.Name = Unescape(rawValue);
                    continue;
                }

                switch (name)
                {
                    case "SUMMARY":
                        current.Summary = Unescape(rawValue);
                        break;
                    case "LOCATION":
                        current.Location = Unescape(rawValue);
                        break;
                    case "DESCRIPTION":
                        current.Description = Unescape(rawValue);
                        break;
                    case "ORGANIZER":
                        current.Organizer = FriendlyAddress(rawValue);
                        break;
                    case "RRULE":
                        current.RecurrenceRule = rawValue;
                        break;
                    case "STATUS":
                        current.Status = rawValue;
                        break;
                    case "DTSTART":
                        current.Start = ParseDate(rawValue, parameters, out var startAllDay);
                        current.AllDay = startAllDay;
                        break;
                    case "DTEND":
                        current.End = ParseDate(rawValue, parameters, out _);
                        break;
                }
            }

            doc.Events.Sort((a, b) => a.Start.CompareTo(b.Start));
            return doc;
        }

        private static IEnumerable<string> Unfold(string text)
        {
            var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
            string current = null;

            foreach (var physical in normalized.Split('\n'))
            {
                if ((physical.StartsWith(" ") || physical.StartsWith("\t")) && current != null)
                {
                    current += physical.Substring(1);
                }
                else
                {
                    if (current != null) yield return current;
                    current = physical;
                }
            }

            if (current != null) yield return current;
        }

        private static int FindPropertyColon(string line)
        {
            bool escaped = false;
            for (int i = 0; i < line.Length; i++)
            {
                if (escaped) { escaped = false; continue; }
                if (line[i] == '\\') { escaped = true; continue; }
                if (line[i] == ':') return i;
            }
            return -1;
        }

        private static DateTime ParseDate(string value, string parameters, out bool allDay)
        {
            allDay = parameters.IndexOf("VALUE=DATE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     value.Length == 8;

            if (allDay)
                return DateTime.ParseExact(value.Substring(0, 8), "yyyyMMdd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None);

            bool utc = value.EndsWith("Z", StringComparison.OrdinalIgnoreCase);
            var trimmed = utc ? value.Substring(0, value.Length - 1) : value;
            var formats = new[] { "yyyyMMdd'T'HHmmss", "yyyyMMdd'T'HHmm" };

            if (!DateTime.TryParseExact(trimmed, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dt))
                return default(DateTime);

            if (utc)
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToLocalTime();

            // Floating and TZID values are displayed as their wall-clock time.
            // A later version can resolve VTIMEZONE/TZID rules fully.
            return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
        }

        private static string FriendlyAddress(string value)
        {
            var v = Unescape(value);
            if (v.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                v = v.Substring(7);
            return v;
        }

        private static string Unescape(string value)
        {
            if (value == null) return null;
            return value
                .Replace("\\n", "\n")
                .Replace("\\N", "\n")
                .Replace("\\,", ",")
                .Replace("\\;", ";")
                .Replace("\\\\", "\\");
        }
    }
}
