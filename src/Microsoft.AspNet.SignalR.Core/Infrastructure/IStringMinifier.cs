// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.md in the project root for license information.

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    public interface IStringMinifier
    {
        /// <summary>
        /// Minifies a string in a way that can be reversed by this instance of <see cref="IStringMinifier"/>.
        /// </summary>
        /// <param name="value">The string to be minified</param>
        /// <returns>A minified representation of the <paramref name="value"/> without the following characters:,|\</returns>
        string Minify(string value);

        /// <summary>
        /// Reverses a <see cref="Minify"/> call that was executed at least once previously on this instance of
        /// <see cref="IStringMinifier"/> without any subsequent calls to <see cref="RemoveUnminified"/> sharing the
        /// same argument as the <see cref="Minify"/> call that returned <paramref name="value"/>.
        /// </summary>
        /// <param name="value">
        /// A minified string that was returned by a previous call to <see cref="Minify"/>.
        /// </param>
        /// <returns>
        /// The argument of all previous calls to <see cref="Minify"/> that returned <paramref name="value"/>.
        /// If every call to <see cref="Minify"/> on this instance of <see cref="IStringMinifier"/> has never
        /// returned <paramref name="value"/> or if the most recent call to <see cref="Minify"/> that did
        /// return <paramref name="value"/> was followed by a call to <see cref="RemoveUnminified"/> sharing 
        /// the same argument, <see cref="Unminify"/> may return null but must not throw.
        /// </returns>
        string Unminify(string value);

        /// <summary>
        /// A call to this function indicates that any future attempt to unminify strings that were previously minified
        /// from <paramref name="value"/> may be met with a null return value. This provides an opportunity clean up
        /// any internal data structures that reference <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The string that may have previously have been minified.</param>
        void RemoveUnminified(string value);
    }
}
