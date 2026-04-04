using Remake;
using Remake.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Remake.Modules
{
    [LuaModule("term")]
    internal static class TermFunctions
    {
        private static int _shouldUseColor = -1;

        private static bool CanUseColors()
        {
            return !Console.IsOutputRedirected;
        }

        private static string GetEnvOrFallback(string var, string fallback)
        {
            return Environment.GetEnvironmentVariable(var) ?? fallback;
        }

        private static bool ShouldUseColors()
        {
            if (_shouldUseColor < 0)
            {
                bool cliColorEnabled = GetEnvOrFallback("CLICOLOR", "1") != "0";
                bool cliForce = GetEnvOrFallback("CLICOLOR_FORCE", "0") != "0";

                _shouldUseColor = (cliForce || (cliColorEnabled && CanUseColors())) ? 1 : 0;
            }

            return _shouldUseColor == 1;
        }

        [LuaFunction("getTextColor", "Return the current console text color.")]
        public static int GetTextColor()
        {
            return (int)Console.ForegroundColor;
        }

        [LuaFunction("setTextColor", "Set the console text color, or reset if negative.")]
        public static void SetTextColor(int color)
        {
            if (color >= 0 && ShouldUseColors())
            {
                if (Enum.IsDefined(typeof(ConsoleColor), color))
                    Console.ForegroundColor = (ConsoleColor)color;
            }
            else if (color < 0)
            {
                Console.ResetColor();
            }
        }
    }
}