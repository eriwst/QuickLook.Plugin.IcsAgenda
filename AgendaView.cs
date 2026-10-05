using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace QuickLook.Plugin.IcsAgenda
{
    internal sealed class AgendaView : UserControl
    {
        private readonly Brush _muted = new SolidColorBrush(Color.FromRgb(112, 112, 112));
        private readonly Brush _line = new SolidColorBrush(Color.FromRgb(220, 220, 220));

        public AgendaView(CalendarDocument calendar)
        {
            var root = new Grid { Margin = new Thickness(28, 22, 28, 22) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var title = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(calendar.Name) ? "Calendar" : calendar.Name,
                FontSize = 22,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 16)
            };
            root.Children.Add(title);

            var scroll = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            Grid.SetRow(scroll, 1);

            var agenda = new StackPanel();
            scroll.Content = agenda;

            if (calendar.Events.Count == 0)
            {
                agenda.Children.Add(new TextBlock
                {
                    Text = "No events found in this .ics file.",
                    Foreground = _muted,
                    FontSize = 14
                });
            }
            else
            {
                foreach (var group in calendar.Events.GroupBy(e => e.Start.Date))
                {
                    AddDayHeader(agenda, group.Key);

                    foreach (var ev in group)
                        AddEvent(agenda, ev);
                }
            }

            root.Children.Add(scroll);
            Content = root;
        }

        private void AddDayHeader(Panel panel, DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            var header = new Border
            {
                BorderBrush = _line,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(0, 14, 0, 8),
                Margin = new Thickness(0, 0, 0, 2),
                Child = new TextBlock
                {
                    Text = date.ToString("dddd, d. MMMM yyyy", culture),
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold
                }
            };
            panel.Children.Add(header);
        }

        private void AddEvent(Panel panel, CalendarEvent ev)
        {
            var grid = new Grid { Margin = new Thickness(0, 10, 0, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var time = new TextBlock
            {
                Text = TimeText(ev),
                FontSize = 13,
                Foreground = _muted,
                Margin = new Thickness(0, 2, 18, 0)
            };
            grid.Children.Add(time);

            var details = new StackPanel();
            Grid.SetColumn(details, 1);

            details.Children.Add(new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(ev.Summary) ? "(Untitled)" : ev.Summary,
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            });

            if (!string.IsNullOrWhiteSpace(ev.Location))
                details.Children.Add(Meta("📍  " + ev.Location));

            if (!string.IsNullOrWhiteSpace(ev.Organizer))
                details.Children.Add(Meta("Organizer: " + ev.Organizer));

            if (!string.IsNullOrWhiteSpace(ev.RecurrenceRule))
                details.Children.Add(Meta("↻  " + HumanizeRecurrence(ev.RecurrenceRule)));

            if (!string.IsNullOrWhiteSpace(ev.Description))
            {
                details.Children.Add(new TextBlock
                {
                    Text = ev.Description,
                    FontSize = 13,
                    Margin = new Thickness(0, 7, 0, 0),
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 680
                });
            }

            grid.Children.Add(details);
            panel.Children.Add(grid);
        }

        private TextBlock Meta(string text) => new TextBlock
        {
            Text = text,
            FontSize = 13,
            Foreground = _muted,
            Margin = new Thickness(0, 4, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };

        private static string TimeText(CalendarEvent ev)
        {
            if (ev.AllDay) return "All day";
            if (ev.End.Date != ev.Start.Date)
                return ev.Start.ToString("HH:mm") + " → " + ev.End.ToString("ddd HH:mm");
            if (ev.End <= ev.Start)
                return ev.Start.ToString("HH:mm");
            return ev.Start.ToString("HH:mm") + " – " + ev.End.ToString("HH:mm");
        }

        private static string HumanizeRecurrence(string rrule)
        {
            if (string.IsNullOrWhiteSpace(rrule))
                return "Recurring event";

            var parts = rrule.Split(';')
                .Select(part => part.Split(new[] { '=' }, 2))
                .Where(pair => pair.Length == 2)
                .ToDictionary(pair => pair[0].Trim().ToUpperInvariant(),
                              pair => pair[1].Trim(),
                              StringComparer.OrdinalIgnoreCase);

            parts.TryGetValue("FREQ", out var frequency);
            parts.TryGetValue("INTERVAL", out var intervalText);

            int interval = 1;
            if (!string.IsNullOrWhiteSpace(intervalText))
                int.TryParse(intervalText, out interval);
            if (interval < 1) interval = 1;

            string summary;
            switch ((frequency ?? string.Empty).ToUpperInvariant())
            {
                case "DAILY":
                    summary = interval == 1 ? "Daily" : "Every " + interval + " days";
                    break;
                case "WEEKLY":
                    summary = interval == 1 ? "Weekly" : "Every " + interval + " weeks";
                    break;
                case "MONTHLY":
                    summary = interval == 1 ? "Monthly" : "Every " + interval + " months";
                    break;
                case "YEARLY":
                    summary = interval == 1 ? "Yearly" : "Every " + interval + " years";
                    break;
                default:
                    summary = "Recurring event";
                    break;
            }

            if (parts.TryGetValue("BYDAY", out var byDay) && !string.IsNullOrWhiteSpace(byDay))
            {
                var dayText = string.Join(", ", byDay.Split(',')
                    .Select(HumanizeWeekday)
                    .Where(day => !string.IsNullOrWhiteSpace(day)));

                if (!string.IsNullOrWhiteSpace(dayText))
                    summary += " · " + dayText;
            }

            if (parts.TryGetValue("COUNT", out var countText) &&
                int.TryParse(countText, out var count) && count > 0)
            {
                summary += " · " + count + (count == 1 ? " event" : " events");
            }

            if (parts.TryGetValue("UNTIL", out var untilText) &&
                TryParseRRuleDate(untilText, out var until))
            {
                summary += " · until " + until.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            }

            return summary;
        }

        private static string HumanizeWeekday(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var code = value.Trim().ToUpperInvariant();
            var suffix = new string(code.Reverse()
                .TakeWhile(char.IsLetter)
                .Reverse()
                .ToArray());

            switch (suffix)
            {
                case "MO": return "Mon";
                case "TU": return "Tue";
                case "WE": return "Wed";
                case "TH": return "Thu";
                case "FR": return "Fri";
                case "SA": return "Sat";
                case "SU": return "Sun";
                default: return value;
            }
        }

        private static bool TryParseRRuleDate(string value, out DateTime date)
        {
            date = default(DateTime);
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var utc = value.EndsWith("Z", StringComparison.OrdinalIgnoreCase);
            var raw = utc ? value.Substring(0, value.Length - 1) : value;
            var formats = new[] { "yyyyMMdd", "yyyyMMdd'T'HHmmss", "yyyyMMdd'T'HHmm" };

            if (!DateTime.TryParseExact(raw, formats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out date))
                return false;

            if (utc)
                date = DateTime.SpecifyKind(date, DateTimeKind.Utc).ToLocalTime();

            return true;
        }
    }
}
