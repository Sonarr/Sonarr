define(['app'], function (app) {
    NzbDrone.Settings.SettingsModel = Backbone.Model.extend({
        url: NzbDrone.Constants.ApiRoot + '/settings'
    });
});