using KeraLua;
using Remake.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Remake
{
    /// <summary>
    /// Specifies that a class represents a Lua module and provides its module name for use in Lua integration.
    /// </summary>
    /// <remarks>Apply this attribute to a class to indicate that it should be exposed as a module in a Lua
    /// environment. The specified name is used as the module identifier when registering or referencing the class from
    /// Lua scripts.</remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LuaModuleAttribute : Attribute
    {
        public string Name { get; }

        public LuaModuleAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Provides methods for registering all static classes marked as Lua modules into a Lua runtime environment.
    /// </summary>
    /// <remarks>This class is intended for use when integrating .NET modules with a Lua scripting
    /// environment. It scans the current assembly for types decorated with the LuaModuleAttribute and registers their
    /// public static methods as Lua functions. Only types and methods explicitly marked with the appropriate attributes
    /// are exposed to Lua. This class is static and cannot be instantiated.</remarks>
    public static class LuaModule
    {
        public static void RegisterAll(Lua L)
        {
            var asm = typeof(LuaModule).Assembly;

            foreach (var type in asm.GetTypes())
            {
                var moduleAttr = type.GetCustomAttribute<LuaModuleAttribute>();
                if (moduleAttr == null)
                    continue;

                RegisterModule(L, moduleAttr.Name, type);
            }
        }

        private static void RegisterModule(Lua L, string moduleName, Type type)
        {
            // Get existing table (string, os, path, etc.)
            L.GetGlobal(moduleName);

            if (!L.IsTable(-1))
            {
                // Create if missing
                L.Pop(1);
                L.NewTable();
                L.SetGlobal(moduleName);
                L.GetGlobal(moduleName);
            }

            // Now stack top = module table
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var fnAttr = method.GetCustomAttribute<LuaFunctionAttribute>();
                if (fnAttr == null)
                    continue;

                // Create trampoline
                var del = LuaDelegateRegistry.Create(
                    $"{moduleName}.{fnAttr.Name}",
                    (state) =>
                    {
                        var lua = Lua.FromIntPtr(state);
                        var binder = LuaVM.Instance.Binder;

                        var parameters = method.GetParameters();
                        object?[] args = new object?[parameters.Length];

                        for (int i = 0; i < parameters.Length; i++)
                            args[i] = binder.ConvertArgument(i + 1, parameters[i].ParameterType);

                        var result = method.Invoke(null, args);

                        if (result == null)
                            return 0;

                        // --- PRIMITIVES ---------------------------------------------------------
                        switch (result)
                        {
                            case bool b:
                                lua.PushBoolean(b);
                                return 1;

                            case string s:
                                lua.PushString(s);
                                return 1;

                            case int n:
                                lua.PushInteger(n);
                                return 1;

                            case double d:
                                lua.PushNumber(d);
                                return 1;
                        }

                        // --- COLLECTIONS → LUA TABLE -------------------------------------------
                        if (result is System.Collections.IEnumerable enumerable && result is not string)
                        {
                            lua.NewTable();
                            int idx = 1;

                            foreach (var item in enumerable)
                            {
                                switch (item)
                                {
                                    case string str: lua.PushString(str); break;
                                    case int iv: lua.PushInteger(iv); break;
                                    case bool bv: lua.PushBoolean(bv); break;
                                    case double dv: lua.PushNumber(dv); break;
                                    default: lua.PushString(item?.ToString() ?? ""); break;
                                }

                                lua.RawSetInteger(-2, idx++);
                            }

                            return 1;
                        }

                        // --- USERDATA (OPAQUE HANDLES) -----------------------------------------
                        if (result is MatchHandle || result is Patterns)
                        {
                            var handle = GCHandle.Alloc(result);
                            lua.PushLightUserData(GCHandle.ToIntPtr(handle));
                            return 1;
                        }

                        // --- OTHER REFERENCE TYPES (rare) --------------------------------------
                        if (!result.GetType().IsValueType)
                        {
                            var handle = GCHandle.Alloc(result);
                            lua.PushLightUserData(GCHandle.ToIntPtr(handle));
                            return 1;
                        }

                        // --- VALUE TYPE FALLBACK ------------------------------------------------
                        lua.PushString(result.ToString());
                        return 1;
                    }
                );

                // Push function into module table
                L.PushCFunction(del);
                L.SetField(-2, fnAttr.Name);
            }

            L.Pop(1); // pop module table
        }
    }
}