using Remake;
using System;
using System.Text;

namespace Remake.Modules
{
    internal static partial class PathFunctions
    {
        private static bool IsSep(char c) => c == '/' || c == '\\';
        private static bool IsQuote(char c) => c == '"' || c == '\'';
        private static bool IsAlpha(char c) => (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
        private static bool IsSpace(char c) => char.IsWhiteSpace(c);

        [LuaFunction("normalize", "Normalize a path according to Premake rules.")]
        public static string Normalize(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            var buffer = new StringBuilder();
            int readPtr = 0;

            // Skip leading whitespace
            while (readPtr < path.Length && IsSpace(path[readPtr]))
                readPtr++;

            while (readPtr < path.Length)
            {
                // Skip tokens like %{...}, $(...), ${...}, %%...%%
                int skipped = SkipTokens(path, readPtr);
                if (skipped > 0)
                {
                    // Preserve separator before token
                    if (readPtr > 0 &&
                        buffer.Length > 0 &&
                        IsSep(path[readPtr - 1]) &&
                        !IsSep(buffer[buffer.Length - 1]))
                    {
                        buffer.Append('/');
                    }

                    buffer.Append(path.Substring(readPtr, skipped));
                    readPtr += skipped;
                }

                // Find end of sub-path (until next space or token)
                int endPtr = readPtr;
                while (endPtr < path.Length &&
                       !IsSpace(path[endPtr]) &&
                       SkipTokens(path, endPtr) == 0)
                {
                    endPtr++;
                }

                if (readPtr != endPtr)
                {
                    // Handle quoted paths
                    if (IsQuote(path[readPtr]))
                        buffer.Append(path[readPtr++]);

                    NormalizeSubstring(path, readPtr, endPtr, buffer);
                }

                // Copy spaces between sub-paths
                while (endPtr < path.Length && IsSpace(path[endPtr]))
                    buffer.Append(path[endPtr++]);

                readPtr = endPtr;
            }

            string result = buffer.ToString().TrimEnd();
            return result.Length == 0 ? "." : result;
        }

        private static int NormalizeSubstring(string src, int srcStart, int srcEnd, StringBuilder dst)
        {
            int srcPtr = srcStart;
            int dstRoot = dst.Length;
            int folderDepth = 0;
            bool isAbsoluteRoot = false;

            // Windows absolute paths (C:)
            if (srcEnd - srcPtr >= 2 && IsAlpha(src[srcPtr]) && src[srcPtr + 1] == ':')
            {
                dst.Append(src[srcPtr++]); // copy drive letter
                dst.Append(src[srcPtr++]); // copy colon AND advance pointer

                // C: is absolute only if followed by a separator
                if (dstRoot == 0 && srcPtr < srcEnd && IsSep(src[srcPtr]))
                    isAbsoluteRoot = true;
            }

            // Leading separators (/ or \\)
            if (srcPtr < srcEnd && IsSep(src[srcPtr]))
            {
                if (dstRoot == 0) isAbsoluteRoot = true;

                srcPtr++;
                dst.Append('/');

                // UNC //
                if (srcPtr < srcEnd && IsSep(src[srcPtr]))
                {
                    srcPtr++;
                    dst.Append('/');
                }
            }

            // The length of the root portion that cannot be backtracked over
            int rootLength = dst.Length;

            while (srcPtr < srcEnd)
            {
                // Skip multiple separators and "./"
                while (srcPtr < srcEnd &&
                      (IsSep(src[srcPtr]) ||
                       (src[srcPtr] == '.' &&
                        (srcPtr + 1 >= srcEnd || IsSep(src[srcPtr + 1])))))
                {
                    srcPtr++;
                }

                if (srcPtr >= srcEnd)
                    break;

                // Handle "../"
                if (src[srcPtr] == '.' &&
                    srcPtr + 1 < srcEnd &&
                    src[srcPtr + 1] == '.' &&
                    (srcPtr + 2 >= srcEnd || IsSep(src[srcPtr + 2])))
                {
                    if (folderDepth > 0)
                    {
                        // Backtrack to previous slash. If we have a trailing slash, remove it first.
                        if (dst.Length > rootLength && IsSep(dst[dst.Length - 1]))
                            dst.Length--;

                        while (dst.Length > rootLength && !IsSep(dst[dst.Length - 1]))
                            dst.Length--;

                        folderDepth--;
                    }
                    else if (!isAbsoluteRoot)
                    {
                        dst.Append("..");
                        dst.Append('/');
                    }

                    srcPtr += 3;
                }
                else
                {
                    // Copy segment
                    while (srcPtr < srcEnd && !IsSep(src[srcPtr]))
                        dst.Append(src[srcPtr++]);

                    if (srcPtr < srcEnd && IsSep(src[srcPtr]))
                    {
                        dst.Append('/');
                        srcPtr++;
                        folderDepth++;
                    }
                }
            }

            // Remove trailing slash except for root
            while (dst.Length > dstRoot && IsSep(dst[dst.Length - 1]))
            {
                if (isAbsoluteRoot && dst.Length <= rootLength)
                    break;

                dst.Length--;
            }

            return dst.Length;
        }

        private static int SkipTokens(string path, int readPtr)
        {
            int start = readPtr;
            bool found;

            do
            {
                found = false;

                if (readPtr + 1 < path.Length)
                {
                    char c1 = path[readPtr];
                    char c2 = path[readPtr + 1];

                    // %{...}, $(...), ${...}, %%...%%
                    if ((c1 == '%' && (c2 == '{' || c2 == '%')) ||
                        (c1 == '$' && (c2 == '(' || c2 == '{')))
                    {
                        char endChar = (c2 == '(') ? ')' :
                                       (c2 == '{') ? '}' : '%';

                        int endIdx = path.IndexOf(endChar, readPtr + 2);
                        if (endIdx != -1)
                        {
                            readPtr = endIdx + 1;
                            found = true;
                        }
                    }
                }

            } while (found && readPtr < path.Length);

            return readPtr - start;
        }
    }
}