using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("rmdir", "Remove an empty directory.")]
        public static object RmDir(string path)
        {
            try
            {
                // Premake semantics: non‑recursive delete, must be empty
                Directory.Delete(path, recursive: false);
                return true; // Lua receives: true
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"unable to remove directory '{path}': {ex.Message}"
                };
            }
        }
    }
}