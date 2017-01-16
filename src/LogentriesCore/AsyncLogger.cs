using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LogentriesCore
{
    public class AsyncLogger
    {
        #region Constants

        // Current version number.
        protected const String Version = "2.6.0";

        // Size of the internal event queue. 
        protected const int QueueSize = 32768;

        // Minimal delay between attempts to reconnect in milliseconds. 
        protected const int MinDelay = 100;

        // Maximal delay between attempts to reconnect in milliseconds. 
        protected const int MaxDelay = 10000;

        // Appender signature - used for debugging messages. 
        protected const String LeSignature = "LE: ";

        // Legacy Logentries configuration names. 
        protected const String LegacyConfigTokenName = "LOGENTRIES_TOKEN";
        protected const String LegacyConfigAccountKeyName = "LOGENTRIES_ACCOUNT_KEY";
        protected const String LegacyConfigLocationName = "LOGENTRIES_LOCATION";

        // New Logentries configuration names.
        protected const String ConfigTokenName = "Logentries.Token";
        protected const String ConfigAccountKeyName = "Logentries.AccountKey";
        protected const String ConfigLocationName = "Logentries.Location";

        // Error message displayed when invalid token is detected. 
        protected const String InvalidTokenMessage = "\n\nIt appears your LOGENTRIES_TOKEN value is invalid or missing.\n\n";

        // Error message displayed when invalid account_key or location parameters are detected. 
        protected const String InvalidHttpPutCredentialsMessage = "\n\nIt appears your LOGENTRIES_ACCOUNT_KEY or LOGENTRIES_LOCATION values are invalid or missing.\n\n";

        // Error message deisplayed when queue overflow occurs. 
        protected const String QueueOverflowMessage = "\n\nLogentries buffer queue overflow. Message dropped.\n\n";

        // Newline char to trim from message for formatting. 
        protected static char[] TrimChars = { '\r', '\n' };

        /** Non-Unix and Unix Newline */
        protected static string[] posix_newline = { "\r\n", "\n" };

        /** Unicode line separator character */
        protected static string line_separator = "\u2028";

        // Restricted symbols that should not appear in host name.
        // See http://support.microsoft.com/kb/228275/en-us for details.
        private static Regex ForbiddenHostNameChars = new Regex(@"[/\\\[\]\""\:\;\|\<\>\+\=\,\?\* _]{1,}", RegexOptions.Compiled);

        /** Regex used to validate GUID in .NET3.5 */
        private static Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        #endregion

        #region Singletons

        // UTF-8 output character set. 
        protected static readonly UTF8Encoding UTF8 = new UTF8Encoding();

        // ASCII character set used by HTTP. 
        protected static readonly ASCIIEncoding ASCII = new ASCIIEncoding();

        //static list of all the queues the le appender might be managing.
        private static ConcurrentBag<BlockingCollection<string>> _allQueues = new ConcurrentBag<BlockingCollection<string>>();

        /// <summary>
        /// Determines if the queue is empty after waiting the specified waitTime.
        /// Returns true or false if the underlying queues are empty.
        /// </summary>
        /// <param name="waitTime">The length of time the method should block before giving up waiting for it to empty.</param>
        /// <returns>True if the queue is empty, false if there are still items waiting to be written.</returns>
        public static bool AreAllQueuesEmpty(TimeSpan waitTime)
        {
            var start = DateTime.UtcNow;
            var then = DateTime.UtcNow;

            while (start.Add(waitTime) > then)
            {
                if (_allQueues.All(x => x.Count == 0))
                    return true;

                Thread.Sleep(100);
                then = DateTime.UtcNow;
            }

            return _allQueues.All(x => x.Count == 0);
        }
        #endregion

        public AsyncLogger()
        {
            Queue = new BlockingCollection<string>(QueueSize);
            _allQueues.Add(Queue);

            WorkerThread = new Thread(Run);
            WorkerThread.Name = "Logentries Log4net Appender";
            WorkerThread.IsBackground = true;
        }

        #region Configuration properties

        private String m_Token = "";
        private String m_AccountKey = "";
        private String m_Location = "";
        private bool m_ImmediateFlush = false;
        private bool m_Debug = false;
        private bool m_UseHttpPut = false;
        private bool m_UseSsl = false;

        // Properties for defining location of DataHub instance if one is used.
        private bool m_UseDataHub = false; // By default Logentries service is used instead of DataHub instance.
        private String m_DataHubAddr = "";
        private int m_DataHubPort = 0;

        // Properties to define host name of user's machine and define user-specified log ID.
        private bool m_UseHostName = false; // Defines whether to prefix log message with HostName or not.
        private String m_HostName = ""; // User-defined or auto-defined host name (if not set in config. file)
        private String m_LogID = ""; // User-defined log ID to be prefixed to the log message.

        // Sets DataHub usage flag.
        public void setIsUsingDataHub(bool useDataHub)
        {
            m_UseDataHub = useDataHub;
        }

        public bool getIsUsingDataHab()
        {
            return m_UseDataHub;
        }

        // Sets DataHub instance address.
        public void setDataHubAddr(String dataHubAddr)
        {
            m_DataHubAddr = dataHubAddr;
        }

        public String getDataHubAddr()
        {
            return m_DataHubAddr;
        }

        // Sets the port on which DataHub instance is waiting for log messages.
        public void setDataHubPort(int port)
        {
            m_DataHubPort = port;
        }

        public int getDataHubPort()
        {
            return m_DataHubPort;
        }

        public void setToken(String token)
        {
            m_Token = token;
        }

        public String getToken()
        {
            return m_Token;
        }

        public void setAccountKey(String accountKey)
        {
            m_AccountKey = accountKey;
        }

        public string getAccountKey()
        {
            return m_AccountKey;
        }

        public void setLocation(String location)
        {
            m_Location = location;
        }

        public String getLocation()
        {
            return m_Location;
        }

        public void setImmediateFlush(bool immediateFlush)
        {
            m_ImmediateFlush = immediateFlush;
        }

        public bool getImmediateFlush()
        {
            return m_ImmediateFlush;
        }

        public void setDebug(bool debug)
        {
            m_Debug = debug;
        }

        public bool getDebug()
        {
            return m_Debug;
        }

        public void setUseHttpPut(bool useHttpPut)
        {
            m_UseHttpPut = useHttpPut;
        }

        public bool getUseHttpPut()
        {
            return m_UseHttpPut;
        }

        public void setUseSsl(bool useSsl)
        {
            m_UseSsl = useSsl;
        }

        public bool getUseSsl()
        {
            return m_UseSsl;
        }

        public void setUseHostName(bool useHostName)
        {
            m_UseHostName = useHostName;
        }

        public bool getUseHostName()
        {
            return m_UseHostName;
        }

        public void setHostName(String hostName)
        {
            m_HostName = hostName;
        }

        public String getHostName()
        {
            return m_HostName;
        }

        public void setLogID(String logID)
        {
            m_LogID = logID;
        }

        public String getLogID()
        {
            return m_LogID;
        }

        #endregion

        protected readonly BlockingCollection<string> Queue;
        protected readonly Thread WorkerThread;
        protected readonly Random Random = new Random();

        private LeClient LeClient = null;
        protected bool IsRunning = false;

        #region Protected methods

        protected virtual void Run()
        {
            try
            {
                // Open connection.
                ReopenConnection();

                string logMessagePrefix = String.Empty;

                if (m_UseHostName)
                {
                    // If LogHostName is set to "true", but HostName is not defined -
                    // try to get host name from Environment.
                    if (m_HostName == String.Empty)
                    {
                        try
                        {
                            WriteDebugMessages("HostName parameter is not defined - trying to get it from System.Environment.MachineName");
                            m_HostName = "HostName=" + System.Environment.MachineName + " ";
                        }
                        catch (InvalidOperationException)
                        {
                            // Cannot get host name automatically, so assume that HostName is not used
                            // and log message is sent without it.
                            m_UseHostName = false;
                            WriteDebugMessages("Failed to get HostName parameter using System.Environment.MachineName. Log messages will not be prefixed by HostName");
                        }
                    }
                    else
                    {
                        if (!CheckIfHostNameValid(m_HostName))
                        {
                            // If user-defined host name is incorrect - we cannot use it
                            // and log message is sent without it.
                            m_UseHostName = false;
                            WriteDebugMessages("HostName parameter contains prohibited characters. Log messages will not be prefixed by HostName");
                        }
                        else
                        {
                            m_HostName = "HostName=" + m_HostName + " ";
                        }
                    }
                }
                            
                if (m_LogID != String.Empty)
                {
                    logMessagePrefix = m_LogID + " ";
                }

                if (m_UseHostName)
                {
                    logMessagePrefix += m_HostName;
                }

                // Flag that is set if logMessagePrefix is empty.
                bool isPrefixEmpty = (logMessagePrefix == String.Empty);

                // Send data in queue.
                while (true)
                {
                    // Take data from queue.
                    var line = Queue.Take();

                    // Replace newline chars with line separator to format multi-line events nicely.
                    foreach (String newline in posix_newline)
                    {
                        line = line.Replace(newline, line_separator);
                    }

                    // If m_UseDataHub == true (logs are sent to DataHub instance) then m_Token is not
                    // appended to the message.
                    string finalLine = ((!m_UseHttpPut && !m_UseDataHub) ? this.m_Token + line : line) + '\n';
                    
                    // Add prefixes: LogID and HostName if they are defined.
                    if (!isPrefixEmpty)
                    {
                        finalLine = logMessagePrefix + finalLine;
                    }

                    byte[] data = UTF8.GetBytes(finalLine);

                    // Send data, reconnect if needed.
                    while (true)
                    {
                        try
                        {
                            this.LeClient.Write(data, 0, data.Length);

                            if (m_ImmediateFlush)
                                this.LeClient.Flush();
                        }
                        catch (IOException)
                        {
                            // Reopen the lost connection.
                            ReopenConnection();
                            continue;
                        }

                        break;
                    }
                }
            }
            catch (ThreadInterruptedException ex)
            {
                WriteDebugMessages("Logentries asynchronous socket client was interrupted.", ex);
            }
        }

        protected virtual void OpenConnection()
        {
            try
            {
                if (LeClient == null)
                {
                    // Create LeClient instance providing all needed parameters. If DataHub-related properties
                    // have not been overridden by log4net or NLog configurators, then DataHub is not used, 
                    // because m_UseDataHub == false by default.
                    LeClient = new LeClient(m_UseHttpPut, m_UseSsl, m_UseDataHub, m_DataHubAddr, m_DataHubPort);
                }                    

                LeClient.Connect();

                if (m_UseHttpPut)
                {
                    var header = String.Format("PUT /{0}/hosts/{1}/?realtime=1 HTTP/1.1\r\n\r\n", m_AccountKey, m_Location);
                    LeClient.Write(ASCII.GetBytes(header), 0, header.Length);
                }
            }
            catch (Exception ex)
            {
                throw new IOException("An error occurred while opening the connection.", ex);
            }
        }

        protected virtual void ReopenConnection()
        {
            CloseConnection();

            var rootDelay = MinDelay;
            while (true)
            {
                try
                {
                    OpenConnection();

                    return;
                }
                catch (Exception ex)
                {
                    if (m_Debug)
                    {
                        WriteDebugMessages("Unable to connect to Logentries API.", ex);
                    }
                }

                rootDelay *= 2;
                if (rootDelay > MaxDelay)
                    rootDelay = MaxDelay;

                var waitFor = rootDelay + Random.Next(rootDelay);

                try
                {
                    Thread.Sleep(waitFor);
                }
                catch
                {
                    throw new ThreadInterruptedException();
                }
            }
        }

        protected virtual void CloseConnection()
        {
            if (LeClient != null)
                LeClient.Close();
        }

        public static bool IsNullOrWhiteSpace(String value)
        {
            if (value == null) return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!Char.IsWhiteSpace(value[i])) return false;
            }

            return true;
        }

        private string retrieveSetting(String name)
        {
            string value;


            value = ConfigurationManager.AppSettings[name];

            if (IsNullOrWhiteSpace(value))
            {
                try
                {
                    value = Environment.GetEnvironmentVariable(name);
                }
                catch (SecurityException)
                {
                }
            }

            return value;
        }

        /*
         * Use CloudConfigurationManager with .NET4.0 and fallback to System.Configuration for previous frameworks.
         * 
         * NOTE: This is not entirely clear with regards to the above comment, but this block of code uses a compiler directive NET4_0
         *       which is not set by default anywhere, so most uses of this code will default back to the "pre-.Net4.0" code branch, even
         *       if you are using .Net4.0 or .Net4.5.
         *       
         *       The second issue is that there are two appsetting keys for each setting - the "legacy" key, such as "LOGENTRIES_TOKEN"
         *       and the "non-legacy" key, such as "Logentries.Token".  Again, I'm not sure of the reasons behind this, so the code below checks
         *       both the legacy and non-legacy keys, defaulting to the legacy keys if they are found.
         *       
         *       It probably should be investigated whether the fallback to ConfigurationManager is needed at all, as CloudConfigurationManager 
         *       will retrieve settings from appSettings in a non-Azure environment.
         */
        protected virtual bool LoadCredentials()
        {
            if (!m_UseHttpPut)
            {
                if (GetIsValidGuid(m_Token))
                    return true;

                var configToken = retrieveSetting(LegacyConfigTokenName) ?? retrieveSetting(ConfigTokenName);

                if (!String.IsNullOrEmpty(configToken) && GetIsValidGuid(configToken))
                {
                    m_Token = configToken;
                    return true;
                }

                WriteDebugMessages(InvalidTokenMessage);
                return false;
            }

            if (m_AccountKey != "" && GetIsValidGuid(m_AccountKey) && m_Location != "")
                return true;

            var configAccountKey = ConfigurationManager.AppSettings[LegacyConfigAccountKeyName] ?? ConfigurationManager.AppSettings[ConfigAccountKeyName];
            if (!String.IsNullOrEmpty(configAccountKey) && GetIsValidGuid(configAccountKey))
            {
                m_AccountKey = configAccountKey;

                var configLocation = ConfigurationManager.AppSettings[LegacyConfigLocationName] ?? ConfigurationManager.AppSettings[ConfigLocationName];
                if (!String.IsNullOrEmpty(configLocation))
                {
                    m_Location = configLocation;
                    return true;
                }
            }

            WriteDebugMessages(InvalidHttpPutCredentialsMessage);
            return false;
        }

        private bool CheckIfHostNameValid(String hostName)
        {
            return !ForbiddenHostNameChars.IsMatch(hostName); // Returns false if reg.ex. matches any of forbidden chars.
        }

        static bool IsGuid(string candidate, out Guid output)
        {
            bool isValid = false;
            output = Guid.Empty;

            if (isGuid.IsMatch(candidate))
            {
                output = new Guid(candidate);
                isValid = true;
            }
            return isValid;
        }

        protected virtual bool GetIsValidGuid(string guidString)
        {
            if (String.IsNullOrEmpty(guidString))
                return false;

            System.Guid newGuid = System.Guid.NewGuid();
            return IsGuid(guidString, out newGuid);
        }

        protected virtual void WriteDebugMessages(string message, Exception ex)
        {
            if (!m_Debug)
                return;

            message = LeSignature + message;
            string[] messages = { message, ex.ToString() };
            foreach (var msg in messages)
            {
                // Use below line instead when compiling with log4net1.2.10.
                //LogLog.Debug(msg);

                //LogLog.Debug(typeof(LogentriesAppender), msg);

                Debug.WriteLine(message);
            }
        }

        protected virtual void WriteDebugMessages(string message)
        {
            if (!m_Debug)
                return;

            message = LeSignature + message;

            // Use below line instead when compiling with log4net1.2.10.
            //LogLog.Debug(message);

            //LogLog.Debug(typeof(LogentriesAppender), message);
            Debug.WriteLine(message);
        }

        #endregion

        #region publicMethods

        public virtual void AddLine(string line)
        {
            if (!IsRunning)
            {
                // We need to load user credentials only
                // if the configuration does not state that DataHub is used;
                // credentials needed only if logs are sent to LE service directly.
                bool credentialsLoaded = false;
                if(!m_UseDataHub)
                {
                    credentialsLoaded = LoadCredentials();
                }

                // If in DataHub mode credentials are ignored.
                if (credentialsLoaded || m_UseDataHub)
                {
                    WriteDebugMessages("Starting Logentries asynchronous socket client.");
                    WorkerThread.Start();
                    IsRunning = true;
                }
            }

            WriteDebugMessages("Queueing: " + line);

            String trimmedEvent = line.TrimEnd(TrimChars);

            // Try to append data to queue.
            if (!Queue.TryAdd(trimmedEvent))
            {
                Queue.Take();
                if (!Queue.TryAdd(trimmedEvent))
                    WriteDebugMessages(QueueOverflowMessage);
            }
        }

        public void interruptWorker()
        {
            WorkerThread.Interrupt();
        }

        #endregion
    }
}