using Remake;
using System;
using System.Runtime.InteropServices;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("getversion", "Return OS version information.")]
        public static object GetVersion()
        {
            var v = Environment.OSVersion.Version;
            string desc = RuntimeInformation.OSDescription;

            return new
            {
                majorversion = v.Major,
                minorversion = v.Minor,
                revision = v.Build,
                description = desc
            };
        }
    }
}