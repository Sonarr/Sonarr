'use strict';
define(['app', 'Settings/Notifications/ItemView'], function () {
    NzbDrone.Settings.Notifications.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Notifications.ItemView,
        itemViewContainer       : 'tbody',
        template                : 'Settings/Notifications/CollectionTemplate'
    });
});
