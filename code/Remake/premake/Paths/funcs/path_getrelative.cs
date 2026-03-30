﻿using Remake;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Remake.Modules
{
    internal static partial class PathFunctions
    {
        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            string normalized = path
                .Replace('\\', '/')
                .TrimEnd('/');

            if (normalized.Length == 2 && normalized[1] == ':')
                normalized += "/";

            return normalized;
        }

        [LuaFunction("getrelative")]
        public static string GetRelative(string p1, string p2)
        {
            string srcAbs = GetAbsolute(p1);
            string dstAbs = GetAbsolute(p2);

            // If either path is empty, fall back to dst
            if (string.IsNullOrEmpty(srcAbs) || string.IsNullOrEmpty(dstAbs))
                return dstAbs ?? "";

            // Ensure directory URIs end with a slash
            if (!srcAbs.EndsWith("/"))
                srcAbs += "/";

            if (!Uri.TryCreate(srcAbs, UriKind.Absolute, out var srcUri))
                return dstAbs;

            if (!Uri.TryCreate(dstAbs, UriKind.Absolute, out var dstUri))
                return dstAbs;

            string rel = Uri.UnescapeDataString(srcUri.MakeRelativeUri(dstUri).ToString());
            return rel.Replace('\\', '/');
        }
    }
}