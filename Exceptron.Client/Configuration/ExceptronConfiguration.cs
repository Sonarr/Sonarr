using System.ComponentModel;
using System.Configuration;
using Exceptron.Client.Message;

namespace Exceptron.Client.Configuration
{

    public class ExceptronConfiguration : ConfigurationSection
    {
        public ExceptronConfiguration()
        {
            Host = "http://exceptron.azurewebsites.net/api/v1/";
            IncludeMachineName = true;
        }

        public static ExceptronConfiguration ReadConfig(string sectionName = "exceptron")
        {
            var configSection = ConfigurationManager.GetSection(sectionName);

            if (configSection == null)
            {
                throw new ConfigurationErrorsException("ExceptronConfiguration section missing.");
            }

            return (ExceptronConfiguration)configSection;
        }


        /// <summary>
        /// exceptron api address. Do not modify this property.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string Host { get; set; }

        /// <summary>
        /// If ExceptronClinet should throw exceptions in case of an error. Default: <see cref="bool.False"/>
        /// </summary>
        /// <remarks>
        /// Its recommended that this flag is set to True during development and <see cref="bool.False"/> in production systems.
        /// If an exception is thrown while this flag is set to <see cref="bool.False"/> the thrown exception will be returned in <see cref="ExceptionResponse.Exception"/>
        /// </remarks>
        [ConfigurationProperty("throwExceptions", DefaultValue = false)]
        public bool ThrowExceptions
        {
            get { return (bool)this["throwExceptions"]; }
            set { this["throwExceptions"] = value; }
        }

        /// <summary>
        /// The API of this application. Can find your API key in application settings page.
        /// </summary>
        [ConfigurationProperty("apiKey")]
        public string ApiKey
        {
            get { return (string)this["apiKey"]; }
            set { this["apiKey"] = value; }
        }


        /// <summary>
        /// If the machine name should be attached to the exception report
        /// </summary>
        /// <remarks>Machine name can be usefull in webfarm enviroments when multiple
        /// servers are running the same app and the issue could be machine specific.
        /// Hoewever, You might want to disable this feature for privacy reasons.</remarks>
        [ConfigurationProperty("includeMachineName", DefaultValue = true)]
        public bool IncludeMachineName
        {
            get { return (bool)this["includeMachineName"]; }
            set { this["includeMachineName"] = value; }
        }

    }
}
