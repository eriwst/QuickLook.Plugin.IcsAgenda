using QuickLook.Common.Plugin;
using System.IO;
using System.Windows;

namespace QuickLook.Plugin.IcsAgenda
{
    public sealed class Plugin : IViewer
    {
        public int Priority => 0;

        public void Init() { }

        public bool CanHandle(string path)
        {
            return !Directory.Exists(path) &&
                   string.Equals(Path.GetExtension(path), ".ics",
                       System.StringComparison.OrdinalIgnoreCase);
        }

        public void Prepare(string path, ContextObject context)
        {
            context.SetPreferredSizeFit(new Size { Width = 900, Height = 760 }, 0.85d);
        }

        public void View(string path, ContextObject context)
        {
            var calendar = IcsParser.ParseFile(path);
            var viewer = new AgendaView(calendar);

            context.ViewerContent = viewer;
            context.Title = Path.GetFileName(path);
            context.IsBusy = false;
        }

        public void Cleanup() { }
    }
}
