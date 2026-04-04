using Remake;
using System;
using System.IO;
using System.Linq;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        // Pure helper — no Lua, no stack, no IntPtr
        private static string? DoPathSearch(string filename, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // Premake uses semicolon-separated lists (not colon)
            string[] parts = path.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
            {
                string testPath = System.IO.Path.Combine(part, filename).Replace('\\', '/');

                if (File.Exists(testPath))
                {
                    // Premake returns the absolute directory containing the file
                    string abs = System.IO.Path.GetFullPath(part)
                                     .Replace('\\', '/')
                                     .TrimEnd('/');

                    return abs;
                }
            }

            return null;
        }

        // Lua-facing function — clean, typed, reflection-friendly
        [LuaFunction("pathsearch", "Search for a file in one or more semicolon-separated paths.")]
        public static string? PathSearch(string filename, params string[] searchPaths)
        {
            foreach (var searchPath in searchPaths)
            {
                if (string.IsNullOrEmpty(searchPath))
                    continue;

                string? result = DoPathSearch(filename, searchPath);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}