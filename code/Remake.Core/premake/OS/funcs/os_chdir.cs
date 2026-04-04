using KeraLua;
using Remake;
using System;
using System.IO;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        public static int DoChDir(string path)
        {
            try
            {
                string buffer = path + "/";
                buffer.Replace("\\", "/");
                Directory.SetCurrentDirectory(buffer);
                return 1; // success (non-zero)
            }
            catch
            {
                return 0; // failure
            }
        }

        [LuaFunction("chdir", "Change the current working directory.")]
        public static int ChDir(string path)
        {
            var L = LuaVM.Instance.State;

            int z = DoChDir(path);

            if (z == 0)
            {
                L.PushNil();
                L.PushString($"unable to switch to directory '{path}'");
                return 2;
            }

            L.PushBoolean(true);
            return 1;
        }
    }
}