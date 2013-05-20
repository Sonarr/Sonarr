'use strict';
define(['app', 'Settings/Notifications/ItemView'], function () {
    NzbDrone.Settings.Notifications.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Notifications.ItemView,
        itemViewContainer       : '#x-notifications',
        template                : 'Settings/Notifications/CollectionTemplate'
    });
});
