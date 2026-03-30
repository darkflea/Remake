using Remake;
using System;
using System.Runtime.InteropServices;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("hostarch", "Return the host CPU architecture.")]
        public static string HostArch()
        {
            // RuntimeInformation.OSArchitecture returns an enum (X64, X86, Arm, Arm64, etc.)
            return RuntimeInformation.OSArchitecture
                                     .ToString()
                                     .ToLowerInvariant();
        }
    }
}