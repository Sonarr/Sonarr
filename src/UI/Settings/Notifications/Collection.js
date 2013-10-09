'use strict';
define(
    [
        'Backbone',
        'Settings/Notifications/Model'
    ], function (Backbone, NotificationModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/notification',
            model: NotificationModel
        });
    });
