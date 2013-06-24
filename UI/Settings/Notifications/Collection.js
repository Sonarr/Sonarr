﻿'use strict';
define(
    [
        'Settings/Notifications/Model'
    ], function (NotificationModel) {
        return Backbone.Collection.extend({
            url  : window.ApiRoot + '/notification',
            model: NotificationModel
        });
    });
