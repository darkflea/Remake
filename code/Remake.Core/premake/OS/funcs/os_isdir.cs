using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("isdir", "Return true if the given path is a directory.")]
        public static bool IsDir(string path)
        {
            // Premake semantics: empty string means "."
            if (string.IsNullOrEmpty(path))
                return true;

            try
            {
                return Directory.Exists(path);
            }
            catch
            {
                // Permission issues, encoding issues, etc.
                return false;
            }
        }
    }
}