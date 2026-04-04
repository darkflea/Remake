using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("rename", "Rename or move a file.")]
        public static object Rename(string fromName, string toName)
        {
            try
            {
                // Premake semantics: overwrite destination if it exists
                File.Move(fromName, toName, overwrite: true);
                return true; // Lua receives: true
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"{fromName}: {ex.Message}",
                    ex.HResult & 0xFFFF // Premake returns Win32-style error codes
                };
            }
        }
    }
}