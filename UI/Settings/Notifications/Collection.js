"use strict";
define(['app', 'Settings/Notifications/Model'], function (App, NotificationModel) {
    return Backbone.Collection.extend({
        url  : App.Constants.ApiRoot + '/notification',
        model: NotificationModel
    });
});
