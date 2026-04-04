using Spectre.Console;
using Spectre.Console.Rendering;

namespace Remake
{
    public class RemakeHelp
    {
        public enum HelpDisplayMode
        {
            Help,
            Version
        }

        private readonly List<string> _lines = new List<string>();

        // Public read-only access to the captured lines
        public IReadOnlyList<string> Lines => _lines;

        public void Add(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                _lines.Add(text);
            }
        }

        public void Display(HelpDisplayMode mode)
        {
            switch (mode)
            {
                case HelpDisplayMode.Help:
                    DisplayHelp();
                    break;

                case HelpDisplayMode.Version:
                    DisplayVersion();
                    break;
            }
        }

        private void DisplayHelp()
        {
            // --- Parse captured lines into categories + actions ---
            var categories = new Dictionary<string, List<string>>();
            var actions = new List<string>();

            string currentCategory = null;

            foreach (var line in _lines)
            {
                if (line.StartsWith("OPTIONS"))
                {
                    currentCategory = line;
                    categories[currentCategory] = new List<string>();
                }
                else if (line.StartsWith("ACTIONS"))
                {
                    currentCategory = "ACTIONS";
                }
                else if (currentCategory == "ACTIONS")
                {
                    actions.Add(line);
                }
                else if (currentCategory != null)
                {
                    categories[currentCategory].Add(line);
                }
            }

            // --- Build a vertical stack of Spectre panels ---
            var panels = new List<IRenderable>();

            // Header panel
            panels.Add(
                new Panel("[bold cyan]Premake Help[/]")
                    .Border(BoxBorder.Rounded)
                    .Expand()
            );

            // Category panels
            foreach (var kvp in categories)
            {
                var text = string.Join("\n", kvp.Value);

                var panel = new Panel(text)
                    .Header($"[yellow]{kvp.Key}[/]")
                    .Border(BoxBorder.Rounded)
                    .Expand();

                panels.Add(panel);
            }

            // Actions table
            if (actions.Count > 0)
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("[bold yellow]Action[/]")
                    .AddColumn("[bold yellow]Description[/]");

                foreach (var line in actions)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                        table.AddRow($"[cyan]{parts[0]}[/]", parts[1]);
                }

                panels.Add(table);
            }

            // Footer panel
            panels.Add(
                new Panel("[grey]ReMake — Powered by Premake DSL[/]")
                    .Border(BoxBorder.Rounded)
                    .Expand()
            );

            // --- Render everything as a vertical stack ---
            var stack = new Rows(panels.ToArray());
            AnsiConsole.Write(stack);
        }

        private void DisplayVersion()
        {
            var versionInfo = new Panel("[bold green]ReMake Version 1.0.0[/]")
                .Border(BoxBorder.Rounded)
                .Expand();
            AnsiConsole.Write(versionInfo);
        }
    }
}