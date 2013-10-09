'use strict';
define(
    [
        'backbone',
        'Settings/Notifications/Model'
    ], function (Backbone, NotificationModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/notification',
            model: NotificationModel
        });
    });
