using Exceptron.Driver.Message;

namespace Exceptron.Driver
{
    public class ClientConfiguration
    {

        public ClientConfiguration()
        {
            ServerUrl = "http://api.exceptron.com/v1a/";
        }

        /// <summary>
        /// If ExceptronClinet should throw exceptions in case of an error. Default: <see cref="bool.False"/>
        /// </summary>
        /// <remarks>
        /// Its recommended that this flag is set to True during development and <see cref="bool.False"/> in production systems.
        /// If an exception is thrown while this flag is set to <see cref="bool.False"/> the thrown exception will be returned in <see cref="ExceptionResponse.Exception"/>
        /// </remarks>
        public bool ThrowsExceptions { get; set; }

        internal string ServerUrl { get; set; }
    }
}
