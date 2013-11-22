// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Owin.Infrastructure
{
    internal static class PrefixMatcher
    {
        public static bool IsMatch(string pathBase, string path)
        {
            pathBase = EnsureStartsWithSlash(pathBase);
            path = EnsureStartsWithSlash(path);

            var pathLength = path.Length;
            var pathBaseLength = pathBase.Length;

            if (pathLength < pathBaseLength)
            {
                return false;
            }

            if (pathLength > pathBaseLength && path[pathBaseLength] != '/')
            {
                return false;
            }

            if (!path.StartsWith(pathBase, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private static string EnsureStartsWithSlash(string path)
        {
            if (path.Length == 0)
            {
                return path;
            }

            if (path[0] == '/')
            {
                return path;
            }

            return '/' + path;
        }
    }
}
