﻿using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("locate", "Locate a file using Premake's search rules.")]
        public static string? Locate(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            // Fetch premake.path from Lua
            var lua = LuaVM.Instance.State;
            lua.GetGlobal("premake");
            lua.GetField(-1, "path");
            string searchPath = lua.ToString(-1) ?? "";
            lua.Pop(2);

            // Embedded script ref ($/foo.lua)
            if (name.StartsWith("$/"))
                return name;

            // Direct file path check (Current Dir)
            if (File.Exists(name))
                return System.IO.Path.GetFullPath(name).Replace('\\', '/');

            // Search premake.path (Search dirs)
            string? located = DoLocate(name, searchPath);
            if (located != null)
                return located;

            // Embedded in exe? (Internal resources)
            var embedded = new LoadEmbeddedScript();
            if (embedded.Find(name) != null)
                return "$/" + name;

            return null;
        }

        private static string? DoLocate(string filename, string searchPath)
        {
            // Ur existing DoPathSearch logic, but returning a string instead of pushing to Lua
            string? dir = DoPathSearch(filename, searchPath);
            if (dir == null)
                return null;

            return $"{dir}/{filename}";
        }
    }
}