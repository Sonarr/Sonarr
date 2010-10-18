// 
// Authors: Nate Kohari <nate@enkari.com>, Remo Gloor <remo.gloor@gmail.com>
// Copyright (c) 2007-2010, Enkari, Ltd. and contributors
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 

namespace Ninject.Web.Mvc
{
    using System.Web.Mvc;

    /// <summary>
    /// Injects all filters of a FiltorInfo.
    /// </summary>
    public interface IFilterInjector
    {
        /// <summary>
        /// Injects all filters of the specified filter info.
        /// </summary>
        /// <param name="filterInfo">The filter info.</param>
        void Inject(FilterInfo filterInfo);
    }
}