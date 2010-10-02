//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.htm file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

#endregion

namespace CassiniDev
{
    internal sealed class SR
    {
        
        internal const string ErrInvalidIPMode="SR.ErrInvalidIPMode";
        internal const string ErrInvalidIPAddress = "ErrInvalidIPAddress";
        internal const string ErrInvalidPortMode = "ErrInvalidPortMode";
        internal const string ErrPortIsInUse = "ErrPortIsInUse";
        internal const string ErrNoAvailablePortFound = "ErrNoAvailablePortFound";
        internal const string ErrPortRangeEndMustBeEqualOrGreaterThanPortRangeSta =
            "ErrPortRangeEndMustBeEqualOrGreaterThanPortRangeSta";
        internal const string ErrInvalidPortRangeValue = "ErrInvalidPortRangeValue";
        internal const string ErrInvalidHostname = "ErrInvalidHostname";

        internal const string ErrFailedToStartCassiniDevServerOnPortError =
            "ErrFailedToStartCassiniDevServerOnPortError";
        internal const string ErrApplicationPathIsNull = "ErrApplicationPathIsNull";
        internal const string ErrPortOutOfRange = "ErrPortOutOfRange";

        internal const string WebdevAspNetVersion = "WebdevAspNetVersion";

        internal const string WebdevDirListing = "WebdevDirListing";

        internal const string WebdevDirNotExist = "WebdevDirNotExist";

        internal const string WebdevErrorListeningPort = "WebdevErrorListeningPort";

        internal const string WebdevHttpError = "WebdevHttpError";

        internal const string WebdevInMemoryLogging = "WebdevInMemoryLogging";

        internal const string WebdevInvalidPort = "WebdevInvalidPort";

        internal const string WebdevLogViewerNameWithPort = "WebdevLogViewerNameWithPort";

        internal const string WebdevName = "WebdevName";

        internal const string WebdevNameWithPort = "WebdevNameWithPort";

        internal const string WebdevOpenInBrowser = "WebdevOpenInBrowser";

        internal const string WebdevRunAspNetLocally = "WebdevRunAspNetLocally";

        internal const string WebdevServerError = "WebdevServerError";

        internal const string WebdevShowDetail = "WebdevShowDetail";

        internal const string WebdevStop = "WebdevStop";

        internal const string WebdevUsagestr1 = "WebdevUsagestr1";

        internal const string WebdevUsagestr2 = "WebdevUsagestr2";

        internal const string WebdevUsagestr3 = "WebdevUsagestr3";

        internal const string WebdevUsagestr4 = "WebdevUsagestr4";

        internal const string WebdevUsagestr5 = "WebdevUsagestr5";

        internal const string WebdevUsagestr6 = "WebdevUsagestr6";

        internal const string WebdevUsagestr7 = "WebdevUsagestr7";

        internal const string WebdevVersionInfo = "WebdevVersionInfo";

        internal const string WebdevVwdName = "WebdevVwdName";

        private static SR _loader;

        private readonly ResourceManager _resources;
        public const string WebdevStart = "WebdevStart";

        internal SR()
        {
            Type t = GetType();
            Assembly thisAssembly = t.Assembly;
            string stringResourcesName = t.Namespace + ".Resources.CassiniDev";
            _resources = new ResourceManager(stringResourcesName, thisAssembly);
        }

        private static CultureInfo Culture
        {
            get { return null; }
        }

        public static ResourceManager Resources
        {
            get { return GetLoader()._resources; }
        }

        public static string GetString(string name)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader._resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader._resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            if (string.IsNullOrEmpty(format))
            {
                return string.Empty;
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        private static SR GetLoader()
        {
            if (_loader == null)
            {
                SR sr = new SR();
                Interlocked.CompareExchange(ref _loader, sr, null);
            }
            return _loader;
        }

    }
}