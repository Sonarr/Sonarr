//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  
//  This source code is subject to terms and conditions of the Microsoft Public
//  License (Ms-PL). A copy of the license can be found in the license.txt file
//  included in this distribution.
//  
//  You must not remove this notice, or any other, from this software.
//  
//  **********************************************************************************

#region

using System;
using System.Runtime.InteropServices;

#endregion

namespace CassiniDev
{
    internal static class Interop
    {
        #region Structs

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int AcceptSecurityContext(ref SecHandle phCredential, IntPtr phContext,
                                                       ref SecBufferDesc pInput, uint fContextReq, uint TargetDataRep,
                                                       ref SecHandle phNewContext, ref SecBufferDesc pOutput,
                                                       ref uint pfContextAttr, ref long ptsTimeStamp);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int AcquireCredentialsHandle(string pszPrincipal, string pszPackage, uint fCredentialUse,
                                                          IntPtr pvLogonID, IntPtr pAuthData, IntPtr pGetKeyFn,
                                                          IntPtr pvGetKeyArgument, ref SecHandle phCredential,
                                                          ref long ptsExpiry);

        [DllImport("KERNEL32.DLL", CharSet = CharSet.Unicode)]
        public static extern int CloseHandle(IntPtr phToken);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int DeleteSecurityContext(ref SecHandle phContext);

        /// <summary>
        /// FIX: #12506
        /// </summary>
        [DllImport("urlmon.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false)]
        public static extern int FindMimeFromData(IntPtr pBC, [MarshalAs(UnmanagedType.LPWStr)] string pwzUrl,
                                                  [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I1,
                                                      SizeParamIndex = 3)] byte[] pBuffer, int cbSize,
                                                  [MarshalAs(UnmanagedType.LPWStr)] string pwzMimeProposed,
                                                  int dwMimeFlags, out IntPtr ppwzMimeOut, int dwReserved);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int FreeCredentialsHandle(ref SecHandle phCredential);

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleScreenBufferInfo", SetLastError = true,
            CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int GetConsoleScreenBufferInfo(int hConsoleOutput,
                                                            ref CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);

        [DllImport("KERNEL32.DLL", SetLastError = true)]
        public static extern IntPtr GetCurrentThread();

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int GetStdHandle(int nStdHandle);

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern bool ImpersonateSelf(int level);

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern int OpenThreadToken(IntPtr thread, int access, bool openAsSelf, ref IntPtr hToken);

        [DllImport("SECUR32.DLL", CharSet = CharSet.Unicode)]
        public static extern int QuerySecurityContextToken(ref SecHandle phContext, ref IntPtr phToken);

        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        public static extern int RevertToSelf();

        #region Nested type: CONSOLE_SCREEN_BUFFER_INFO

        public struct CONSOLE_SCREEN_BUFFER_INFO
        {
            internal COORD dwCursorPosition;
            internal COORD dwMaximumWindowSize;
            internal COORD dwSize;
            internal SMALL_RECT srWindow;
            internal Int16 wAttributes;
        }

        #endregion

        #region Nested type: COORD

        public struct COORD
        {
            internal Int16 x;
            internal Int16 y;
        }

        #endregion

        #region Nested type: SecBuffer

        [StructLayout(LayoutKind.Sequential)]
        public struct SecBuffer
        {
            // ReSharper disable InconsistentNaming
            public uint cbBuffer;
            public uint BufferType;
            public IntPtr pvBuffer;
            // ReSharper restore InconsistentNaming
        }

        #endregion

        #region Nested type: SecBufferDesc

        [StructLayout(LayoutKind.Sequential)]
        public struct SecBufferDesc
        {
            // ReSharper disable InconsistentNaming
            public uint ulVersion;
            public uint cBuffers;
            public IntPtr pBuffers;
            // ReSharper restore InconsistentNaming
        }

        #endregion

        #region Nested type: SecHandle

        [StructLayout(LayoutKind.Sequential)]
        public struct SecHandle
        {
            // ReSharper disable InconsistentNaming
            public IntPtr dwLower;
            public IntPtr dwUpper;
            // ReSharper restore InconsistentNaming
        }

        #endregion

        #region Nested type: SMALL_RECT

        public struct SMALL_RECT
        {
            internal Int16 Bottom;
            internal Int16 Left;
            internal Int16 Right;
            internal Int16 Top;
        }

        #endregion

        #endregion
    }
}