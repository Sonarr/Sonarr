"use strict";
define(['app'], function () {

    $.cookie.json = true;

    NzbDrone.Config.SeriesView = function (value) {
        if (value) {
            NzbDrone.Config.SetValue('seriesView', value);
        }

        else{
            return NzbDrone.Config.GetValue('seriesView', 0);
        }
    };

    NzbDrone.Config.GetValue = function (key, defaultValue) {
        var cookie = NzbDrone.Config.GetCookie();
        var value =  cookie[key];

        if (!value) {
            return defaultValue;
        }

        else {
            return value;
        }
    };

    NzbDrone.Config.SetValue = function (key, value) {
        var cookie = NzbDrone.Config.GetCookie();
        cookie[key] = value;
    };

    NzbDrone.Config.GetCookie = function () {
        return $.cookie('NzbDroneConfig');
    };

    NzbDrone.Config.SetCookie = function (cookie) {
        $.cookie('NzbDroneConfig', cookie, { expires: 7, path: '/' });
    };
});
