'use strict';
define(['app', 'Shared/NotificationModel'], function () {

    var notificationCollection = Backbone.Collection.extend({
        model: NzbDrone.Shared.NotificationModel
    });

    return new notificationCollection();
});


