using KeraLua;
using System.Text;

namespace Remake
{
    public class LoadEmbeddedScript
    {
#if DEBUG
        private static bool _warned = false;
#endif

        private string m_filename = null;

        public LoadEmbeddedScript()
        {
        }

        /// <summary>
        /// Locate a file in the embedded script index.
        /// Returns the mapping if found, otherwise null.
        /// </summary>
        public BuildinMapping? Find(string filename)
        {
            // Simple linear search matching the original C logic
            foreach (var script in PremakeDSL.EmbeddedLua.BuiltinScripts)
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
        public LuaStatus Load(Lua l, string filename)
        {
            var bytecode = PremakeDSL.EmbeddedLua.GetBytes(filename);
            Console.WriteLine(Encoding.UTF8.GetString(bytecode));

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