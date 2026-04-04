using Remake;
using System;
using System.Text;

namespace Remake.Modules
{
    internal static partial class OSFunctions
    {
        [LuaFunction("uuid", "Generate a UUID, optionally based on a name.")]
        public static string Uuid(string? name = null)
        {
            Guid guid;

            if (name != null)
            {
                // Build the 16‑byte buffer using the DBJ2‑style hashing
                byte[] bytes = new byte[16];

                WriteUint32(bytes, 0, DoHash(name, 0));
                WriteUint32(bytes, 4, DoHash(name, (uint)'L'));
                WriteUint32(bytes, 8, DoHash(name, (uint)'u'));
                WriteUint32(bytes, 12, DoHash(name, (uint)'a'));

                guid = new Guid(bytes);
            }
            else
            {
                // Random UUID
                guid = Guid.NewGuid();
            }

            // Premake uses uppercase canonical form
            return guid.ToString("D").ToUpperInvariant();
        }

        private static void WriteUint32(byte[] buffer, int offset, uint value)
        {
            // Little‑endian packing (matches Premake C)
            buffer[offset] = (byte)(value & 0xFF);
            buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
            buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
            buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
        }

        private static uint DoHash(string name, uint seed)
        {
            // DBJ2 variant used by Premake
            uint hash = 5381 + seed;
            foreach (char c in name)
                hash = ((hash << 5) + hash) + (uint)c;
            return hash;
        }
    }
}