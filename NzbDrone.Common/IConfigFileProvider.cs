using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NzbDrone.Common.Model;

namespace NzbDrone.Common
{
    public interface IConfigFileProvider
    {
        Guid Guid { get; }
        int Port { get; set; }
        bool LaunchBrowser { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string BasicAuthUsername { get; set; }
        string BasicAuthPassword { get; set; }
        int GetValueInt(string key, int defaultValue);
        bool GetValueBoolean(string key, bool defaultValue);
        string GetValue(string key, object defaultValue);
        void SetValue(string key, object value);
    }

    public class ConfigFileProvider : IConfigFileProvider
    {
        private readonly IEnvironmentProvider _environmentProvider;

        private readonly string _configFile;

        public ConfigFileProvider(IEnvironmentProvider environmentProvider)
        {
            _environmentProvider = environmentProvider;
            _configFile = _environmentProvider.GetConfigPath();

            CreateDefaultConfigFile();
        }

        public virtual Guid Guid
        {
            get
            {
                var key = "Guid";
                if (string.IsNullOrWhiteSpace(GetValue(key, string.Empty)))
                {
                    SetValue(key, Guid.NewGuid().ToString());
                }

                return Guid.Parse(GetValue(key, string.Empty));
            }
        }

        public virtual int Port
        {
            get { return GetValueInt("Port", 8989); }
            set { SetValue("Port", value); }
        }

        public virtual bool LaunchBrowser
        {
            get { return GetValueBoolean("LaunchBrowser", true); }
            set { SetValue("LaunchBrowser", value); }
        }

        public virtual AuthenticationType AuthenticationType
        {
            get { return GetValueEnum("AuthenticationType", AuthenticationType.Anonymous); }
            set { SetValue("AuthenticationType", value); }
        }

        public virtual string BasicAuthUsername
        {
            get { return GetValue("BasicAuthUsername", ""); }
            set { SetValue("BasicAuthUsername", value); }
        }

        public virtual string BasicAuthPassword
        {
            get { return GetValue("BasicAuthPassword", ""); }
            set { SetValue("BasicAuthPassword", value); }
        }

        public virtual int GetValueInt(string key, int defaultValue)
        {
            return Convert.ToInt32(GetValue(key, defaultValue));
        }

        public virtual bool GetValueBoolean(string key, bool defaultValue)
        {
            return Convert.ToBoolean(GetValue(key, defaultValue));
        }

        private T GetValueEnum<T>(string key, T defaultValue)
        {
            return (T)Enum.Parse(typeof(T), GetValue(key, defaultValue), true);
        }

        public virtual string GetValue(string key, object defaultValue)
        {
            var xDoc = XDocument.Load(_configFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;


            var valueHolder = parentContainer.Descendants(key).ToList();

            if (valueHolder.Count() == 1)
                return valueHolder.First().Value;

            //Save the value
            SetValue(key, defaultValue);

            //return the default value
            return defaultValue.ToString();
        }

        public virtual void SetValue(string key, object value)
        {
            var xDoc = XDocument.Load(_configFile);
            var config = xDoc.Descendants("Config").Single();

            var parentContainer = config;

            var keyHolder = parentContainer.Descendants(key);

            if (keyHolder.Count() != 1)
                parentContainer.Add(new XElement(key, value));

            else
                parentContainer.Descendants(key).Single().Value = value.ToString();

            xDoc.Save(_configFile);
        }

        private void SetValue(string key, Enum value)
        {
            SetValue(key, value.ToString().ToLower());
        }

        private void CreateDefaultConfigFile()
        {
            if (!File.Exists(_configFile))
            {
                var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));

                xDoc.Add(new XElement("Config"));

                xDoc.Save(_configFile);
            }
        }
    }
}
