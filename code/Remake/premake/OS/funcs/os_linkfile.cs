using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("linkfile", "Create a symbolic link to a file.")]
        public static object LinkFile(string src, string dst)
        {
            try
            {
                File.CreateSymbolicLink(dst, src);
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