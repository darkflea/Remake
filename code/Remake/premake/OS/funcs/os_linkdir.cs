using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("linkdir", "Create a symbolic link to a directory.")]
        public static object LinkDir(string src, string dst)
        {
            try
            {
                Directory.CreateSymbolicLink(dst, src);
                return true; // Lua receives: true
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"Unable to create link from '{src}' to '{dst}': {ex.Message}"
                };
            }
        }
    }
}