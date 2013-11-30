﻿'use strict';

define(
    [
        'marionette',
        'Calendar/UpcomingCollection',
        'Calendar/UpcomingItemView',
        'Mixins/backbone.signalr.mixin'
    ], function (Marionette, UpcomingCollection, UpcomingItemView) {
        return Marionette.CollectionView.extend({
            itemView: UpcomingItemView,

            initialize: function () {
                this.collection = new UpcomingCollection().bindSignalR({ updateOnly: true });
                this.collection.fetch();
            }
        });
    });
