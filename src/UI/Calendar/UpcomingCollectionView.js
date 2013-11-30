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
                this.collection = new UpcomingCollection().bindSignalR();
                this.collection.fetch();

                this.listenTo(this.collection, 'change', this._refresh);
            },

            _refresh: function () {
                this.render();
            }
        });
    });
