using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remake
{
    public class RemakeHelp
    {
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

        public void Display()
        {
            // Your Spectre.Console logic goes here
            foreach (var line in _lines)
            {
                Console.WriteLine(line);
            }
        }
    }
}