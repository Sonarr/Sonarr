using NzbDrone.Core.Configuration;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.Config;

public class UiSettingsResource : RestResource
{
    // Calendar
    public int FirstDayOfWeek { get; set; }
    public string? CalendarWeekColumnHeader { get; set; }

    // Dates
    public string? ShortDateFormat { get; set; }
    public string? LongDateFormat { get; set; }
    public string? TimeFormat { get; set; }
    public string? TimeZone { get; set; }
    public bool ShowRelativeDates { get; set; }

    public bool EnableColorImpairedMode { get; set; }
    public string? Theme { get; set; }
    public int UiLanguage { get; set; }
}

public static class UiSettingsResourceMapper
{
    public static UiSettingsResource ToResource(IConfigFileProvider config, IConfigService model)
    {
        return new UiSettingsResource
        {
            FirstDayOfWeek = model.FirstDayOfWeek,
            CalendarWeekColumnHeader = model.CalendarWeekColumnHeader,

            ShortDateFormat = model.ShortDateFormat,
            LongDateFormat = model.LongDateFormat,
            TimeFormat = model.TimeFormat,
            TimeZone = model.TimeZone,
            ShowRelativeDates = model.ShowRelativeDates,

            EnableColorImpairedMode = model.EnableColorImpairedMode,
            Theme = config.Theme,
            UiLanguage = model.UILanguage
        };
    }
}
