using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Remake.Modules
{
    internal partial class OverrideFunctions
    {
        public static readonly LuaFunction LuaBDofileDelegate = PremakeLuaBDofile;

        /// <summary>
        /// Port of premake_luaB_dofile.
        /// Replaces the standard Lua 'dofile' function.
        /// </summary>
        public static int PremakeLuaBDofile(IntPtr state)
        {
            var l = Lua.FromIntPtr(state);
            string? fname = l.OptString(1, null);
            LuaStatus status = PremakeLuaLLoadFileX(l, fname, null);

            if (status != LuaStatus.OK)
            {
                return l.Error();
            }

            l.Call(0, Lua.MultiRet);
            return l.GetTop() - 1;
        }

        // Port of #define premake_luaL_dofile(L, fn)
        public static LuaStatus PremakeLuaLDoFile(Lua l, string filename)
        {
            LuaStatus status = PremakeLuaLLoadFileX(l, filename, null);

            if (status == LuaStatus.OK)
            {
                status = l.PCall(0, Lua.MultiRet, 0);
            }

            return status;
        }
    }
}