﻿'use strict';
define(
    [
        'Settings/Notifications/Model'
    ], function (NotificationModel) {
        return Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/notification',
            model: NotificationModel
        });
    });
