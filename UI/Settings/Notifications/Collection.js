"use strict";
define(['app', 'Settings/Notifications/Model'], function () {
    NzbDrone.Settings.Notifications.Collection = Backbone.Collection.extend({
        url  : NzbDrone.Constants.ApiRoot + '/notification',
        model: NzbDrone.Settings.Notifications.Model
    });
});
