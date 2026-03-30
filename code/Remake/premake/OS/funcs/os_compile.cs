using KeraLua;
using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("compile", "Compile a Lua script into bytecode.")]
        public static object Compile(string input, string output)
        {
            try
            {
                // Load the script text
                string source = File.ReadAllText(input);

                // Compile to bytecode using your VM
                var lua = LuaVM.Instance.State;
                var status = lua.LoadString(source, input);

                if (status != LuaStatus.OK)
                {
                    string msg = lua.ToString(-1) ?? "(error with no message)";
                    return new object?[]
                    {
                        null,
                        $"Unable to compile '{input}': {msg}"
                    };
                }

                // Dump bytecode
                byte[] bytes = lua.Dump();

                // Write to output file
                File.WriteAllBytes(output, bytes);

                return true;
            }
            catch (Exception ex)
            {
                return new object?[]
                {
                    null,
                    $"unable to write to '{output}': {ex.Message}"
                };
            }
        }
    }
}