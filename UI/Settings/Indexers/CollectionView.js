'use strict';

define(['app', 'Settings/Indexers/ItemView'], function (app) {

    NzbDrone.Settings.Indexers.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Indexers.ItemView,
        itemViewContainer       : '#x-indexers',
        template                : 'Settings/Indexers/CollectionTemplate'
    });
});