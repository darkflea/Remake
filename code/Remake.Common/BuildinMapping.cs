namespace Remake
{
    public struct BuildinMapping
    {
        public string Name;      // logical script name (e.g. "xcode/xcode.lua")
        public byte[] Bytecode;  // embedded stripped/bytecode data
        public int Length;       // cached length for convenience

        public BuildinMapping(string name, byte[] bytecode)
        {
            Name = name;
            Bytecode = bytecode;
            Length = bytecode.Length;
        }
    }
}