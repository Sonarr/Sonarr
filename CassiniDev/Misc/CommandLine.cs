//  **********************************************************************************
//  CassiniDev - http://cassinidev.codeplex.com
// 
//  Copyright (c) 2010 Sky Sanders. All rights reserved.
//  Copyright (c) Microsoft Corporation. All rights reserved.
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
using System.Collections;
using System.Collections.Specialized;

#endregion

namespace CassiniDev.UIComponents
{
    public sealed class CommandLine
    {
        #region Fields

        private readonly string[] _arguments;

        private readonly bool _showHelp;
        private IDictionary _options;

        #endregion

        #region Constructors

        public CommandLine(string[] args)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < args.Length; i++)
            {
                char ch = args[i][0];
                if ((ch != '/') && (ch != '-'))
                {
                    list.Add(args[i]);
                }
                else
                {
                    int index = args[i].IndexOf(':');
                    if (index == -1)
                    {
                        string strA = args[i].Substring(1);
                        if ((string.Compare(strA, "help", StringComparison.OrdinalIgnoreCase) == 0) || strA.Equals("?"))
                        {
                            _showHelp = true;
                        }
                        else
                        {
                            Options[strA] = string.Empty;
                        }
                    }
                    else
                    {
                        Options[args[i].Substring(1, index - 1)] = args[i].Substring(index + 1);
                    }
                }
            }
            _arguments = (string[]) list.ToArray(typeof (string));
        }

        #endregion

        #region Properties

        public string[] Arguments
        {
            get { return _arguments; }
        }

        public IDictionary Options
        {
            get
            {
                if (_options == null)
                {
                    _options = new HybridDictionary(true);
                }
                return _options;
            }
        }

        public bool ShowHelp
        {
            get { return _showHelp; }
        }

        #endregion
    }
}