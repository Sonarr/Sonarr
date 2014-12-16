using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;

namespace LogentriesCore
{
    class LeClient
    {
        // Logentries API server address. 
        protected const String LeApiUrl = "api.logentries.com";

        // Port number for token logging on Logentries API server. 
        protected const int LeApiTokenPort = 10000;

        // Port number for TLS encrypted token logging on Logentries API server 
        protected const int LeApiTokenTlsPort = 20000;

        // Port number for HTTP PUT logging on Logentries API server. 
        protected const int LeApiHttpPort = 80;

        // Port number for SSL HTTP PUT logging on Logentries API server. 
        protected const int LeApiHttpsPort = 443;

        // Creates LeClient instance. If do not define useServerUrl and/or useOverrideProt during call
        // LeClient will be configured to work with api.logentries.com server; otherwise - with
        // defined server on defined port.
        public LeClient(bool useHttpPut, bool useSsl, bool useDataHub, String serverAddr, int port)
        {
            
            // Override port number and server address to send logs to DataHub instance.
            if (useDataHub)
            {
                m_UseSsl = false; // DataHub does not support receiving log messages over SSL for now.
                m_TcpPort = port;
                m_ServerAddr = serverAddr;
            }
            else
            {
                m_UseSsl = useSsl;

                if (!m_UseSsl)
                    m_TcpPort = useHttpPut ? LeApiHttpPort : LeApiTokenPort;
                else
                    m_TcpPort = useHttpPut ? LeApiHttpsPort : LeApiTokenTlsPort;
            }            
        }

        private bool m_UseSsl = false;
        private int m_TcpPort;
        private TcpClient m_Client = null;
        private Stream m_Stream = null;
        private SslStream m_SslStream = null;
        private String m_ServerAddr = LeApiUrl; // By default m_ServerAddr points to api.logentries.com if useDataHub is not set to true.

        private Stream ActiveStream
        {
            get
            {
                return m_UseSsl ? m_SslStream : m_Stream;
            }
        }

        public void Connect()
        {
            m_Client = new TcpClient(m_ServerAddr, m_TcpPort);
            m_Client.NoDelay = true;

            m_Stream = m_Client.GetStream();

            if (m_UseSsl)
            {
                m_SslStream = new SslStream(m_Stream);
                m_SslStream.AuthenticateAsClient(m_ServerAddr);

            }
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            ActiveStream.Write(buffer, offset, count);
        }

        public void Flush()
        {
            ActiveStream.Flush();
        }

        public void Close()
        {
            if (m_Client != null)
            {
                try
                {
                    m_Client.Close();
                }
                catch
                {
                }
            }
        }
    }
}
