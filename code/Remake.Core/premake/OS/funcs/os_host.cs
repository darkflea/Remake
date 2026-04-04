using Remake;
using System;
using System.Runtime.InteropServices;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("host", "Return the host operating system identifier.")]
        public static string Host()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return "windows";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return "macosx";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
                return "bsd";

            return "unknown";
        }
    }
}