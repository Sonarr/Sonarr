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
using CassiniDev.ServerLog;

#endregion

namespace CassiniDev
{
    public class RequestEventArgs : EventArgs
    {
        private readonly Guid _id;

        private readonly LogInfo _requestLog;

        private readonly LogInfo _responseLog;

        public RequestEventArgs(Guid id, LogInfo requestLog, LogInfo responseLog)
        {
            _requestLog = requestLog;
            _responseLog = responseLog;
            _id = id;
        }

        public Guid Id
        {
            get { return _id; }
        }

        public LogInfo RequestLog
        {
            get { return _requestLog; }
        }

        public LogInfo ResponseLog
        {
            get { return _responseLog; }
        }
    }
}