using KeraLua;
using Remake.DSL;
using System;

namespace Remake
{
    public static class PremakeLoader
    {
#if DEBUG
        private static bool _warned = false;
#endif

        /// <summary>
        /// This represents your global registry of scripts
        /// Accesses the EmbeddedLua.Map dictionary defined in your DSL layer.
        /// </summary>
        public static List<BuildinMapping> BuiltinScripts = new List<BuildinMapping>();

        /// <summary>
        /// Locate a file in the embedded script index.
        /// Returns the mapping if found, otherwise null.
        /// </summary>
        public static BuildinMapping? FindEmbeddedScript(string filename)
        {
            // Simple linear search matching the original C logic
            foreach (var script in BuiltinScripts)
            {
                if (script.Name == filename)
                {
                    return script;
                }
            }
            return null;
        }

        /// <summary>
        /// Port of premake_load_embedded_script.
        /// Loads the bytecode from the DSL Map into the Lua state.
        /// </summary>
        public static LuaStatus PremakeLoadEmbeddedScript(Lua l, string filename)
        {
            var bytecode = EmbeddedLua.GetBytes(filename);

            if (bytecode.Length == 0)
            {
                return LuaStatus.ErrFile;
            }

#if DEBUG
            if (!_warned)
            {
                _warned = true;
                Console.WriteLine($"** warning: using embedded script '{filename}'; use /scripts argument to load from files");
            }
#endif

            var qualifiedName = "$/" + filename;
            l.PushString(qualifiedName);

            // Load the chunk into Lua
            var result = l.LoadBuffer(bytecode, filename);
            return result;
        }
    }
}