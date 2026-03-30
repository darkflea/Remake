// Auto-generated from AnvilEmbed (behavioural port of Premake embed.lua)
// DO NOT EDIT MANUALLY
namespace Remake.DSL
{
    public static class EmbeddedLua
    {
        public static readonly BuildinMapping[] BuiltinScripts = new BuildinMapping[]
        {
        };

        public static readonly Dictionary<string, BuildinMapping> Map = new Dictionary<string, BuildinMapping>(System.StringComparer.OrdinalIgnoreCase)
        {
        };

        public static byte[] GetBytes(string name)
        {
            foreach (var s in BuiltinScripts)
            {
                if (System.StringComparer.OrdinalIgnoreCase.Equals(s.Name, name))
                    return s.Bytecode;
            }
            return System.Array.Empty<byte>();
        }
    }
}
