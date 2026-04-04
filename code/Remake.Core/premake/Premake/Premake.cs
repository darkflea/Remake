using Remake;
using Remake.Modules;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Remake.Modules
{
    [LuaModule("premake")]
    internal static class EmbeddedResourceFunctions
    {
        [LuaFunction("getEmbeddedResource", "Return an embedded Lua script as a binary-safe string.")]
        public static object? GetEmbeddedResource(string filename)
        {
            byte[] bytes = PremakeDSL.EmbeddedLua.GetBytes(filename);

            if (bytes.Length == 0)
                return null; // Lua receives nil

            return bytes; // Lua receives a binary-safe string
        }
    }
}