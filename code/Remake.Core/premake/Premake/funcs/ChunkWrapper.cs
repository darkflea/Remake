﻿using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remake.Modules
{
    internal partial class OverrideFunctions
    {
        // Make sure these delegates are static so they aint garbage collected
        public static readonly LuaFunction ChunkWrapperDelegate = LuaCallbackRegistry.CreateCallback("chunk_wrapper", ChunkWrapper);

        /// <summary>
        /// Port of chunk_wrapper (C function).
        /// This is a Lua C-Function that wraps exec of a script chunk.
        /// </summary>
        /// <param name="luaState">Raw pointer to lua_State.</param>
        /// <returns>Number of return values on stack.</returns>
        private static int ChunkWrapper(IntPtr luaState)
        {
            var l = Lua.FromIntPtr(luaState);
            int args = l.GetTop();
            int upvalueOffset = (l.Type(Lua.UpValueIndex(1)) == LuaType.Table) ? 1 : 0;
            string oldCwd = OSFunctions.do_getcwd();// Directory.GetCurrentDirectory();
            l.GetGlobal("_SCRIPT");
            l.GetGlobal("_SCRIPT_DIR");
            l.PushValue(Lua.UpValueIndex(1 + upvalueOffset));
            string filename = l.ToString(-1);
            l.SetGlobal("_SCRIPT");
            l.PushString(filename.Replace('\\', '/'));
            l.SetGlobal("_SCRIPT");

            // Compute dir
            string directory = Path.GetDirectoryName(filename)!;
            string luaDirectory = directory.Replace('\\', '/');
            if (luaDirectory.Length > 0 && !luaDirectory.EndsWith("/")) luaDirectory += "/";
            l.PushString(luaDirectory);
            l.SetGlobal("_SCRIPT_DIR");

            // Always chdir to script dir
            OSFunctions.DoChDir(directory);

            // Function is at upvalue index (2 + offset)
            l.PushValue(Lua.UpValueIndex(2 + upvalueOffset));

            // env table if it exists
            if (upvalueOffset == 1)
            {
                l.PushValue(-1);                // Push function again to setup upvalue
                l.PushValue(Lua.UpValueIndex(1)); // Push the env table
                l.SetUpValue(-2, 1);           // Set env as 1st upvalue of the function
            }

            // function args to top of stack
            for (int i = 1; i <= args; i++)
            {
                l.PushValue(i);
            }

            // Execute chunk
            LuaStatus status = l.PCall(args, Lua.MultiRet, 0);
            OSFunctions.DoChDir(oldCwd);

            // Restore _SCRIPT global from saved stack value (index args + 1)
            l.PushValue(args + 1);
            l.SetGlobal("_SCRIPT");

            // Restore _SCRIPT_DIR global from saved stack value (index args + 2)
            l.PushValue(args + 2);
            l.SetGlobal("_SCRIPT_DIR");

            // If script failed, re-throw error now that we cleaned up
            if (status != LuaStatus.OK)
            {
                l.Error(); // Uses the error message at the top of the stack
            }

            // return reults
            return l.GetTop() - args - 2;
        }
    }
}