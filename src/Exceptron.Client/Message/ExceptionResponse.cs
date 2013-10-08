using System;
using Exceptron.Client.Configuration;

namespace Exceptron.Client.Message
{
    public class ExceptionResponse
    {
        /// <summary>
        /// Exception report reference ID. This ID will be shared across
        /// similar exceptions
        /// </summary>
        public string RefId { get; internal set; }

        /// <summary>
        /// Was the report successfully processed on the server
        /// </summary>
        public bool Successful
        {
            get
            {
                return !string.IsNullOrEmpty(RefId);
            }
        }

        /// <summary>
        /// Exception that caused the message to fail. 
        /// </summary>
        /// <remarks>
        /// This property will only be populated if <see cref="ExceptronConfiguration.ThrowExceptions"/> is set to <see cref="bool.False"/>/>
        /// Exception is thrown if <see cref="ExceptronConfiguration.ThrowExceptions"/> is set to <see cref="bool.True"/>.
        /// </remarks>
        public Exception Exception { get; internal set; }
    }
}