'use strict';

define([
    'app', 'Settings/SettingsModel'

], function () {

    NzbDrone.Settings.Notifications.NotificationsView = Backbone.Marionette.ItemView.extend({
        template: 'Settings/Notifications/NotificationsTemplate',
    });
});
