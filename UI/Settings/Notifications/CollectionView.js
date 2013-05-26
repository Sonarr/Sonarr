'use strict';
define(['app', 'Settings/Notifications/ItemView', 'Settings/Notifications/AddView'], function () {
    NzbDrone.Settings.Notifications.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Notifications.ItemView,
        itemViewContainer       : 'tbody',
        template                : 'Settings/Notifications/CollectionTemplate',

        events: {
            'click .x-add': 'openSchemaModal'
        },

        openSchemaModal: function () {
            var schema = new NzbDrone.Settings.Notifications.Collection();
            schema.url = '/api/notification/schema';
            schema.fetch();

            var view = new NzbDrone.Settings.Notifications.AddView({ collection: schema});
            NzbDrone.modalRegion.show(view);
        }
    });
});
