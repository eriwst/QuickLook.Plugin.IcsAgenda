using System;
using System.Collections.Generic;

namespace QuickLook.Plugin.IcsAgenda
{
    internal sealed class CalendarDocument
    {
        public string Name { get; set; }
        public List<CalendarEvent> Events { get; } = new List<CalendarEvent>();
    }

    internal sealed class CalendarEvent
    {
        public string Summary { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool AllDay { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Organizer { get; set; }
        public string RecurrenceRule { get; set; }
        public string Status { get; set; }
    }
}
