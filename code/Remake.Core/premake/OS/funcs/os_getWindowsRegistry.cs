#if WINDOWS
using Microsoft.Win32;
using Remake;
using System.Runtime.InteropServices;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("getwindowsregistry", "Read a value from the Windows Registry.")]
        public static object? GetWindowsRegistry(string fullPath)
        {
            try
            {
                int colonIndex = fullPath.IndexOf(':');
                if (colonIndex == -1)
                    return null;

                string prefix = fullPath[..colonIndex].ToUpperInvariant();
                string remain = fullPath[(colonIndex + 1)..].TrimStart('\\');

                RegistryKey? root = prefix switch
                {
                    "HKCU" => Registry.CurrentUser,
                    "HKLM" => Registry.LocalMachine,
                    "HKCR" => Registry.ClassesRoot,
                    "HKU" => Registry.Users,
                    "HKCC" => Registry.CurrentConfig,
                    _ => null
                };

                if (root == null)
                    return null;

                int lastSlash = remain.LastIndexOf('\\');
                string subKeyPath = lastSlash >= 0 ? remain[..lastSlash] : "";
                string valueName = lastSlash >= 0 ? remain[(lastSlash + 1)..] : remain;

                using RegistryKey? key = root.OpenSubKey(subKeyPath);
                object? val = key?.GetValue(valueName);

                return val?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
#endif