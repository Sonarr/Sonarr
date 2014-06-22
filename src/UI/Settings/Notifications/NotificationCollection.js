'use strict';

define([
    'backbone',
    'Settings/Notifications/NotificationModel'
], function (Backbone, NotificationModel) {

    return Backbone.Collection.extend({
        model: NotificationModel,
        url  : window.NzbDrone.ApiRoot + '/notification'

    });
});
