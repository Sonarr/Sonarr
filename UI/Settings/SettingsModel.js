"use strict";
define(['app'], function () {
    NzbDrone.Settings.SettingsModel = Backbone.Model.extend({
        url: NzbDrone.Constants.ApiRoot + '/settings'
    });
});
