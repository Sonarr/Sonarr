using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NzbDrone.Core
{
    public static class Fluent
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetDiskFreeSpaceEx(string lpDirectoryName,
        out ulong lpFreeBytesAvailable,
        out ulong lpTotalNumberOfBytes,
        out ulong lpTotalNumberOfFreeBytes);

        public static string WithDefault(this string actual, object defaultValue)
        {
            if (defaultValue == null)
                throw new ArgumentNullException("defaultValue");
            if (String.IsNullOrWhiteSpace(actual))
            {
                return defaultValue.ToString();
            }

            return actual;
        }

        public static Int64 Megabytes(this int megabytes)
        {
            return megabytes * 1048576L;
        }

        public static Int64 Gigabytes(this int gigabytes)
        {
            return gigabytes * 1073741824L;
        }

        public static string ToBestDateString(this DateTime dateTime)
        {
            if (dateTime == DateTime.Today.AddDays(-1))
                return "Yesterday";

            if (dateTime == DateTime.Today)
                return "Today";

            if (dateTime == DateTime.Today.AddDays(1))
                return "Tomorrow";

            if (dateTime > DateTime.Today.AddDays(1) && dateTime < DateTime.Today.AddDays(7))
                return dateTime.DayOfWeek.ToString();

            return dateTime.ToShortDateString();
        }


        //TODO: this should be moved to DiskProvider
        public static ulong FreeDiskSpace(this DirectoryInfo directoryInfo)
        {
            ulong freeBytesAvailable;
            ulong totalNumberOfBytes;
            ulong totalNumberOfFreeBytes;

            bool success = GetDiskFreeSpaceEx(directoryInfo.FullName, out freeBytesAvailable, out totalNumberOfBytes,
                               out totalNumberOfFreeBytes);
            if (!success)
                throw new System.ComponentModel.Win32Exception();

            return freeBytesAvailable;
        }
    }
}
