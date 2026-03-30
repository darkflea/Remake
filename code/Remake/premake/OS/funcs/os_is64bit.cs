using Remake;
using System;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("is64bit", "Return true if the operating system is 64-bit.")]
        public static bool Is64Bit()
        {
            // Matches Premake semantics: check the OS, not the process
            return Environment.Is64BitOperatingSystem;
        }
    }
}