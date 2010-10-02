using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

namespace CassiniDev.Configuration
{
    public class CassiniDevConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("profiles")]
        public CassiniDevProfileElementCollection Profiles
        {
            get
            {
                return (CassiniDevProfileElementCollection)this["profiles"];
            }
        }
    }

    [ConfigurationCollection(typeof(CassiniDevProfileElement), AddItemName = "profile", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class CassiniDevProfileElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CassiniDevProfileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CassiniDevProfileElement)element).Port;
        }
    }
    public class CassiniDevProfileElement : ConfigurationElement
    {
        /// <summary>
        /// Port is used as profile selector
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = "*", IsKey = true, IsRequired = true)]
        public string Port
        {
            get
            {
                return (string)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        [ConfigurationProperty("path")]
        public string Path
        {
            get
            {
                return (string)this["path"];
            }
            set
            {
                this["path"] = value;
            }
        }




        [ConfigurationProperty("hostName")]
        public string HostName
        {
            get
            {
                return (string)this["hostName"];
            }
            set
            {
                this["hostName"] = value;
            }
        }

        [ConfigurationProperty("ip")]
        public string IpAddress
        {
            get
            {
                return (string)this["ip"];
            }
            set
            {
                this["ip"] = value;
            }
        }

        [ConfigurationProperty("ipMode", DefaultValue = CassiniDev.IPMode.Loopback)]
        public IPMode IpMode
        {
            get
            {
                return (IPMode)this["ipMode"];
            }
            set
            {
                this["ipMode"] = value;
            }
        }

        [ConfigurationProperty("v6", DefaultValue = false)]
        public bool IpV6
        {
            get
            {
                return (bool)this["v6"];
            }
            set
            {
                this["v6"] = value;
            }
        }
    }
}
