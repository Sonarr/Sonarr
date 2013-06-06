namespace Exceptron.Client.Message
{
    internal class Frame
    {
        /// <summary>
        /// Order of current frame
        /// </summary>
        public int i { get; set; }

        /// <summary>
        /// Line number of the current frame
        /// </summary>
        public int ln { get; set; }

        /// <summary>
        /// File name of the current frame
        /// </summary>
        public string fn { get; set; }

        /// <summary>
        /// Method name for current frame
        /// </summary>
        public string m { get; set; }

        /// <summary>
        /// Class name for current frame
        /// </summary>
        public string c { get; set; }
    }
}