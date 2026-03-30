using KeraLua;
using System.Collections.Generic;

namespace Remake
{
    /// <summary>
    /// A registry for Lua functions that need to be pinned to prevent them from being garbage collected.
    /// </summary>
    internal static class LuaDelegateRegistry
    {
        private static readonly List<LuaFunction> _pinned = new();
        private static readonly Dictionary<LuaFunction, string> _names = new();

        public static LuaFunction Create(string name, Func<IntPtr, int> func)
        {
            LuaFunction del = null;

            del = new LuaFunction((state) =>
            {
                return func(state);
            });

            _pinned.Add(del);
            _names[del] = name;

            return del;
        }

        public static string? GetName(LuaFunction fn)
            => _names.TryGetValue(fn, out var name) ? name : null;
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class LuaCallbackRegistry
    {
        public static readonly List<LuaFunction> Pinned = new();
        public static readonly Dictionary<LuaFunction, string> Names = new();

        public static LuaFunction CreateCallback(string name, Func<IntPtr, int> func)
        {
            LuaFunction del = null;

            del = new LuaFunction((state) =>
            {
                return func(state);
            });

            LuaCallbackRegistry.Pinned.Add(del);
            LuaCallbackRegistry.Names[del] = name;

            return del;
        }
    }
}