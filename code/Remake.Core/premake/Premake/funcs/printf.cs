using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Remake.Modules
{
    internal partial class OverrideFunctions
    {
        // This holds the context for the current execution
        public static RemakeHelp CurrentHelpContext { get; set; }

        public static readonly LuaFunction LuaPrintFDelegate = PrintF;

        public static int PrintF(IntPtr state)
        {
            var l = LuaVM.Instance.State;
            int argCount = l.GetTop();
            int retcode = 0;

            if (argCount == 0) return 0;
            if (argCount == 1)
            {
                retcode = ProcessOutput(l.ToString(1));
                return 0;
            }

            // Push 'string.format' onto the stack
            l.GetGlobal("string");
            l.GetField(-1, "format");
            l.Remove(-2); // remove the 'string' table, leaving 'format' function

            // Push all the original arguments to 'string.format'
            for (int i = 1; i <= argCount; i++)
            {
                l.PushValue(i);
            }

            // Call string.format(args...)
            // 1 result expected (the formatted string)
            if (l.PCall(argCount, 1, 0) != LuaStatus.OK)
            {
                string err = l.ToString(-1);
                retcode = ProcessOutput($"[Lua Format Error]: {err}");
                return 0;
            }

            // Get the result and store it
            string formatted = l.ToString(-1);
            l.Pop(1); // clear result

            retcode = ProcessOutput(formatted);
            return retcode;
        }

        private static int ProcessOutput(string text)
        {
            if (CurrentHelpContext != null)
            {
                CurrentHelpContext.Add(text);
            }
            else
            {
                // Default behavior if help isn't active
                Console.Write(text);
            }
            return 0;
        }
    }
}