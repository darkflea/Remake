#if WINDOWS
using Microsoft.Win32;
using Remake;
using System;
using System.Collections.Generic;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("listwindowsregistry", "List subkeys and values under a Windows registry path.")]
        public static object? ListWindowsRegistry(string fullPath)
        {
            try
            {
                int colonIndex = fullPath.IndexOf(':');
                if (colonIndex == -1)
                    return null;

                string prefix = fullPath[..colonIndex].ToUpperInvariant();
                string subKeyPath = fullPath[(colonIndex + 1)..].TrimStart('\\');

                RegistryKey? root = prefix switch
                {
                    "HKCU" => Registry.CurrentUser,
                    "HKLM" => Registry.LocalMachine,
                    "HKCR" => Registry.ClassesRoot,
                    "HKU" => Registry.Users,
                    "HKCC" => Registry.CurrentConfig,
                    _ => null
                };

                using RegistryKey? key = root?.OpenSubKey(subKeyPath);
                if (key == null)
                    return null;

                var result = new Dictionary<string, object?>();

                //
                // 1. Subkeys → empty tables
                //
                foreach (string subkeyName in key.GetSubKeyNames())
                {
                    result[subkeyName] = new Dictionary<string, object?>();
                }

                //
                // 2. Values → { type = "...", value = ... }
                //
                foreach (string valueName in key.GetValueNames())
                {
                    RegistryValueKind kind = key.GetValueKind(valueName);
                    object? rawValue = key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);

                    result[valueName] = new Dictionary<string, object?>
                    {
                        ["type"] = kind.ToString().ToUpperInvariant(),
                        ["value"] = ConvertRegistryValue(kind, rawValue)
                    };
                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        private static object? ConvertRegistryValue(RegistryValueKind kind, object? val)
        {
            if (val == null)
                return null;

            return kind switch
            {
                RegistryValueKind.String or RegistryValueKind.ExpandString
                    => val.ToString(),

                RegistryValueKind.DWord
                    => (int)(long)val,

                RegistryValueKind.QWord
                    => (long)val,

                RegistryValueKind.MultiString
                    => (string[])val,

                RegistryValueKind.Binary
                    => (byte[])val,

                _ => null
            };
        }
    }
}
#endif