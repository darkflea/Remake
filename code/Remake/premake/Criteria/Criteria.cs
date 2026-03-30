using KeraLua;
using System;
using System.Collections.Generic;

namespace Remake.Modules
{
    public class Word
    {
        public string? Text;
        public string? Prefix;
        public bool MatchesFiles;
        public bool Assertion;
        public bool Wildcard;
    }

    public class Pattern
    {
        public bool MatchesFiles;
        public List<Word> Words { get; } = new();
    }

    public class Patterns
    {
        public bool Prefixed;
        public int FilePatterns;
        public List<Pattern> Items { get; } = new();
    }

    // DTO that matches the Lua word table shape
    public class WordDto
    {
        public string? word { get; set; }
        public string? prefix { get; set; }
        public bool assertion { get; set; }
        public bool wildcard { get; set; }
    }

    [LuaModule("criteria")]
    internal static class CriteriaFunctions
    {
        /// <summary>
        /// Compile raw criteria patterns from Lua into a more efficient structure for matching.
        /// </summary>
        /// <param name="rawPatterns"></param>
        /// <returns></returns>
        [LuaFunction("_compile", "Compile criteria patterns into a reusable structure.")]
        public static Patterns Compile(List<List<WordDto>> rawPatterns)
        {
            var ps = new Patterns();

            foreach (var rawPattern in rawPatterns)
            {
                var p = new Pattern();

                foreach (var w in rawPattern)
                {
                    var word = new Word
                    {
                        Text = w.word,
                        Prefix = w.prefix,
                        Assertion = w.assertion,
                        Wildcard = w.wildcard,
                        MatchesFiles = w.prefix == "files"
                    };

                    if (word.MatchesFiles)
                        p.MatchesFiles = true;

                    p.Words.Add(word);
                }

                ps.Items.Add(p);

                if (p.Words.Count > 0 && p.Words[0].Prefix != null)
                    ps.Prefixed = true;

                if (p.MatchesFiles)
                    ps.FilePatterns++;
            }

            return ps;
        }

        [LuaFunction("matches", "Test compiled criteria against a context table.")]
        public static bool Matches(object patterns, Dictionary<string, object?> context, Func<string, string, string?> stringMatch)
        {
            // 1. Handle the "Match All" shorthand (Lua sends 1 or 1.0)
            if (patterns is double d && d == 1.0) return true;
            if (patterns is int i && i == 1) return true;

            // 2. Handle null or missing (default to match)
            if (patterns == null)
                return true;

            // 3. Handle the actual Patterns class
            if (patterns is Patterns p)
            {
                context.TryGetValue("files", out var fileVal);
                string? filename = fileVal as string;

                bool fileMatched = filename == null;
                bool overallMatched = true;

                if (p.Prefixed && filename != null && p.FilePatterns == 0)
                    return false;

                foreach (var patternItem in p.Items) // Changed name to avoid conflict with 'patterns' arg
                {
                    bool patternMatched = false;
                    foreach (var w in patternItem.Words)
                    {
                        bool res = w.Prefix != null
                            ? TestWithPrefix(w, filename, context, ref fileMatched, stringMatch)
                            : TestNoPrefix(w, filename, context, ref fileMatched, stringMatch);

                        if (res)
                        {
                            patternMatched = true;
                            break;
                        }
                    }

                    if (!patternMatched)
                    {
                        overallMatched = false;
                        break;
                    }
                }

                if (filename != null && !fileMatched)
                    overallMatched = false;

                return overallMatched;
            }

            // Fallback for unknown types
            return true;
        }

        private static bool MatchString(string value, Word word, Func<string, string, string?> stringMatch)
        {
            if (!word.Wildcard)
                return value == word.Text;

            var result = stringMatch(value, word.Text ?? string.Empty);
            return result != null && result == value;
        }

        private static bool TestValue(object? value, Word word, Func<string, string, string?> stringMatch)
        {
            if (value is null)
                return false;

            if (value is string s)
                return MatchString(s, word, stringMatch);

            if (value is IList<object?> list)
            {
                foreach (var item in list)
                {
                    if (item is string si && MatchString(si, word, stringMatch))
                        return true;
                }
                return false;
            }

            // Fallback: try ToString
            return MatchString(value.ToString() ?? string.Empty, word, stringMatch);
        }

        private static bool TestWithPrefix(
            Word word,
            string? filename,
            Dictionary<string, object?> context,
            ref bool fileMatched,
            Func<string, string, string?> stringMatch)
        {
            if (word.MatchesFiles && filename == null)
                return false;

            context.TryGetValue(word.Prefix!, out var value);
            bool result = TestValue(value, word, stringMatch);

            if (word.MatchesFiles && result == word.Assertion)
                fileMatched = true;

            return result ? word.Assertion : !word.Assertion;
        }

        private static bool TestNoPrefix(
            Word word,
            string? filename,
            Dictionary<string, object?> context,
            ref bool fileMatched,
            Func<string, string, string?> stringMatch)
        {
            if (filename != null && word.Assertion && MatchString(filename, word, stringMatch))
            {
                fileMatched = true;
                return true;
            }

            foreach (var kvp in context)
            {
                if (TestValue(kvp.Value, word, stringMatch))
                    return word.Assertion;
            }

            return !word.Assertion;
        }
    }
}