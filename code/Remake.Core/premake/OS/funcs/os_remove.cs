using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("remove", "Remove a file from disk.")]
        public static object Remove(string filename)
        {
            try
            {
                // Premake semantics: removing a missing file is an error
                if (!File.Exists(filename))
                    throw new FileNotFoundException();

                File.Delete(filename);
                return true; // Lua receives: true
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"{filename}: {ex.Message}",
                    ex.HResult & 0xFFFF // Premake returns the Win32-style error code
                };
            }
        }
    }
}