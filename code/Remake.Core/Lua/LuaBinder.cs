using KeraLua;
using Remake.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Remake
{
    /// <summary>
    /// Converts Lua arguments to C# types based on expected parameter types.
    /// </summary>
    public class LuaBinder
    {
        private readonly Lua lua;

        public LuaBinder(Lua _lua)
        {
            lua = _lua;
        }

        /// <summary>
        /// Converts a Lua argument at the given index to the specified C# type.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public object? ConvertArgument(int index, Type targetType)
        {
            if (index > lua.GetTop())
                return null;

            // string
            if (targetType == typeof(string))
                return lua.ToString(index);

            // --- USERDATA HANDLING --------------------------------------------------
            // Premake uses *lightuserdata* for opaque handles (MatchHandle, Patterns, etc.)
            // KeraLua:
            //   - ToUserData() works ONLY for full userdata
            //   - ToPointer() works for lightuserdata
            // So we must check both paths explicitly.

            if (lua.IsUserData(index))
            {
                // Full userdata (rare in your host, but supported)
                IntPtr ptr = lua.ToUserData(index);
                if (ptr != IntPtr.Zero)
                {
                    var handle = GCHandle.FromIntPtr(ptr);
                    var obj = handle.Target;

                    if (obj != null && targetType.IsAssignableFrom(obj.GetType()))
                        return obj;

                    Console.WriteLine($"[LuaBinder] Full userdata type mismatch at index {index}. Expected {targetType.Name}, got {obj?.GetType().Name ?? "null"}");

                    return null;
                }
            }

            if (lua.IsLightUserData(index))
            {
                // Lightuserdata (Premake-style opaque handles)
                IntPtr ptr = lua.ToPointer(index);
                if (ptr != IntPtr.Zero)
                {
                    var handle = GCHandle.FromIntPtr(ptr);
                    var obj = handle.Target;

                    if (obj == null)
                    {
                        Console.WriteLine($"[LuaBinder] LightUserData at index {index} resolved to null target.");
                        return null;
                    }

                    // MatchHandle
                    if (targetType == typeof(MatchHandle))
                        return (MatchHandle)obj;

                    // Patterns
                    if (targetType == typeof(Patterns))
                        return (Patterns)obj;

                    // Generic GCHandle-backed object
                    if (targetType.IsAssignableFrom(obj.GetType()))
                        return obj;

                    Console.WriteLine($"[LuaBinder] LightUserData type mismatch at index {index}. Expected {targetType.Name}, got {obj.GetType().Name}");

                    return null;
                }
            }

            // ------------------------------------------------------------
            // PARAMS ARRAY HANDLING (Lua varargs → C# params object[])
            // ------------------------------------------------------------
            if (targetType.IsArray && targetType.GetElementType() == typeof(object))
            {
                int count = lua.GetTop();
                var arr = new object[count];

                for (int i = 0; i < count; i++)
                    arr[i] = ConvertDynamic(i + 1);

                return arr;
            }

            // nil -> null
            if (lua.IsNil(index))
                return null;

            // bool
            if (targetType == typeof(bool))
                return lua.ToBoolean(index);

            // double
            if (targetType == typeof(double))
                return lua.ToNumber(index);

            // int
            if (targetType == typeof(int))
                return (int)lua.ToInteger(index);

            // string[]
            if (targetType == typeof(string[]))
                return ConvertStringArray(index);

            // object
            if (targetType == typeof(object))
            {
                var val = ConvertDynamic(index);
                Console.WriteLine($"[LuaBinder] Dynamic conversion at index {index}: {val} (type: {val?.GetType().Name ?? "null"})");
                return val;
            }

            // Dictionary<string, object?>
            if (targetType == typeof(Dictionary<string, object?>))
                return ConvertDictionary(index);

            // List<T>
            if (targetType.IsGenericType &&
                targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = targetType.GetGenericArguments()[0];
                return ConvertList(index, elementType);
            }

            if (targetType == typeof(WordDto))
                return ConvertWordDto(index);

            var luaType = lua.Type(index);
            Console.WriteLine($"[LuaBinder] Warning: Type mismatch at index {index}. Lua type '{luaType}' does not match expected '{targetType.Name}'.");
            return null;
        }

        /// <summary>
        /// Determines if the Lua value at the given index is an array-like table (i.e., has a numeric key starting at 1).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private bool IsArray(int index)
        {
            if (!lua.IsTable(index))
                return false;

            // Convert relative index (like -1) to absolute so it doesn't 
            // change meaning after we push things onto the stack
            int abs = lua.AbsIndex(index);

            // Look for the first element (Lua arrays start at 1)
            lua.PushInteger(1);
            lua.GetTable(abs);

            bool hasIndexOne = !lua.IsNil(-1);

            // Pop the result of GetTable to keep the stack clean
            lua.Pop(1);

            return hasIndexOne;
        }

        /// <summary>
        /// Converts a Lua value at the given index to a string array. If it's a single string, it returns an array with that string.
        /// If it's a table, it converts the table values to an array of strings.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string[] ConvertStringArray(int index)
        {
            // Single string -> [s]
            if (lua.IsString(index))
                return new[] { lua.ToString(index)! };

            // Table of strings -> array
            if (!lua.IsTable(index))
                return Array.Empty<string>();

            int abs = lua.AbsIndex(index);
            var list = new List<string>();

            lua.PushNil();
            while (lua.Next(abs))
            {
                string? s = lua.ToString(-1);
                if (s != null)
                    list.Add(s);
                lua.Pop(1);
            }

            return list.ToArray();
        }

        /// <summary>
        /// Converts a Lua table at the given index to a Dictionary<string, object?>.
        /// It iterates over the table's key-value pairs and adds them to the dictionary.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Dictionary<string, object?> ConvertDictionary(int index)
        {
            var dict = new Dictionary<string, object?>();

            if (!lua.IsTable(index))
                return dict;

            int abs = lua.AbsIndex(index);

            lua.PushNil();
            while (lua.Next(abs))
            {
                string? key = lua.ToString(-2);
                if (key != null)
                    dict[key] = ConvertDynamic(-1);

                lua.Pop(1);
            }

            return dict;
        }

        /// <summary>
        /// Dynamically converts a Lua value at the given index to an appropriate C# type (string, bool, number, list, etc.).
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private object? ConvertDynamic(int index)
        {
            if (lua.IsNil(index))
                return null;

            if (lua.IsString(index))
                return lua.ToString(index);

            if (lua.IsBoolean(index))
                return lua.ToBoolean(index);

            if (lua.IsNumber(index))
                return lua.ToNumber(index);

            if (lua.IsTable(index))
                return ConvertList(index, typeof(object));

            return null;
        }

        /// <summary>
        /// Converts a Lua table at the given index to a List<T> where T is the specified element type.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="elementType"></param>
        /// <returns></returns>
        private object ConvertList(int index, Type elementType)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (System.Collections.IList)Activator.CreateInstance(listType)!;

            if (!lua.IsTable(index))
                return list;

            int abs = lua.AbsIndex(index);

            lua.PushNil();
            while (lua.Next(abs))
            {
                object? value = ConvertArgument(-1, elementType);
                list.Add(value);
                lua.Pop(1);
            }

            return list;
        }

        /// <summary>
        /// Converts a Lua table at the given index to a Remake.Modules.Criteria.WordDto.
        /// It reads specific fields from the table and populates the DTO properties accordingly.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Remake.Modules.WordDto ConvertWordDto(int index)
        {
            var dto = new Remake.Modules.WordDto();

            if (!lua.IsTable(index))
                return dto;

            int abs = lua.AbsIndex(index);

            lua.PushNil();
            while (lua.Next(abs))
            {
                string? key = lua.ToString(-2);

                switch (key)
                {
                    case "word": dto.word = lua.ToString(-1); break;
                    case "prefix": dto.prefix = lua.ToString(-1); break;
                    case "assertion": dto.assertion = lua.ToBoolean(-1); break;
                    case "wildcard": dto.wildcard = lua.ToBoolean(-1); break;
                }

                lua.Pop(1);
            }

            return dto;
        }
    }
}