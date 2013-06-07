'use strict';

define(['app', 'Calendar/UpcomingItemView'], function () {
    NzbDrone.Calendar.UpcomingCollectionView = Backbone.Marionette.CollectionView.extend({
        itemView: NzbDrone.Calendar.UpcomingItemView
    });
});
