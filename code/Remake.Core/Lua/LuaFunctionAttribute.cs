using System;

namespace Remake
{
    /// <summary>
    /// An attribute to mark methods as Lua functions that should be registered with the Lua environment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class LuaFunctionAttribute : Attribute
    {
        public string Name { get; }
        public string? Description { get; }

        public LuaFunctionAttribute(string name, string? description = null)
        {
            Name = name;
            Description = description;
        }
    }
}