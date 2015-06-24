using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.JDownloader
{
    public enum JDownloaderPriority
    {
        Low = 0,
        Default = 1,
        High = 2,
        Higher = 3,
        Highest = 4
    }
    public class JDownloaderSettingsValidator : AbstractValidator<JDownloaderSettings>
    {
        public JDownloaderSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).GreaterThan(0);
            RuleFor(c => c.LinkCheckerTimeout).GreaterThan(0).LessThan(120);
            //RuleFor(c => c.Username).NotEmpty().When(c => !String.IsNullOrWhiteSpace(c.Password));
            //RuleFor(c => c.Password).NotEmpty().When(c => !String.IsNullOrWhiteSpace(c.Username));

            //RuleFor(c => c.TvCategory).NotEmpty().WithMessage("A category is recommended").AsWarning();
        }
    }
    public class JDownloaderSettings : IProviderConfig
    {
        private static readonly JDownloaderSettingsValidator Validator = new JDownloaderSettingsValidator();

        public JDownloaderSettings()
        {
            Host = "localhost";
            Port = 3128;
            RecentTvPriority = (int)JDownloaderPriority.Default;
            OlderTvPriority = (int)JDownloaderPriority.Default;
            LinkCheckerTimeout = 15;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public String Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public Int32 Port { get; set; }

        //[FieldDefinition(2, Label = "Username", Type = FieldType.Textbox)]
        //public String Username { get; set; }

        //[FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        //public String Password { get; set; }

        [FieldDefinition(5, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(JDownloaderPriority), HelpText = "Priority to use when grabbing episodes that aired within the last 14 days")]
        public Int32 RecentTvPriority { get; set; }

        [FieldDefinition(6, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(JDownloaderPriority), HelpText = "Priority to use when grabbing episodes that aired over 14 days ago")]
        public Int32 OlderTvPriority { get; set; }

        [FieldDefinition(7, Label = "Linkchecker Timeout (in s)", Type = FieldType.Textbox)]
        public int LinkCheckerTimeout { get; set; }
        
        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}