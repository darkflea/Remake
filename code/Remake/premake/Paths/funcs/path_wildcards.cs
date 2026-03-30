﻿using Remake;
using System;
using System.Text;

namespace Remake.Modules
{
    internal static partial class PathFunctions
    {
        private const int MaxWildcardExpansion = 0x4000;

        [LuaFunction("wildcards", "Convert Premake-style wildcards to Lua patterns.")]
        public static string Wildcards(string pattern)
        {
            Console.WriteLine("Im feel really wild baby!");
            if (string.IsNullOrEmpty(pattern))
                return "";

            // Normalize slashes to forward slash (Premake does this)
            pattern = pattern.Replace('\\', '/');

            var sb = new StringBuilder(pattern.Length * 2);

            for (int i = 0; i < pattern.Length; ++i)
            {
                char c = pattern[i];

                switch (c)
                {
                    // Escape Lua magic characters
                    case '%':
                    case '.':
                    case '+':
                    case '-':
                    case '^':
                    case '$':
                    case '(':
                    case ')':
                        sb.Append('%').Append(c);
                        break;

                    // Character classes: keep [ and ] exactly as-is
                    case '[':
                    case ']':
                        sb.Append(c);
                        break;

                    // Wildcards
                    case '*':
                        // Handle ** (recursive)
                        if (i + 1 < pattern.Length && pattern[i + 1] == '*')
                        {
                            sb.Append(".*");
                            i++; // skip second '*'
                        }
                        else
                        {
                            // Single * = match anything except slash
                            sb.Append("[^/]*");
                        }
                        break;

                    case '?':
                        // Single character wildcard
                        sb.Append('.');
                        break;

                    default:
                        sb.Append(c);
                        break;
                }
            }

            // Premake anchors the pattern
            return "^" + sb.ToString() + "$";
        }
    }
}