namespace Exceptron.Driver
{
    /// <summary>
    /// Severity of the exception being reported
    /// </summary>
    public enum ExceptionSeverity
    {
        /// <summary>
        /// Excepted Error. Can be ignored
        /// </summary>
        None = 0,

        /// <summary>
        /// Error that can be handled gracefully
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Blocking user from completing their intended action
        /// </summary>
        Error = 2,

        /// <summary>
        /// Will most likely cause the application to crash
        /// </summary>
        Fatal = 3
    }
}