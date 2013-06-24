﻿'use strict';

define(
    [
        'marionette',
        'Calendar/UpcomingItemView'
    ], function (Marionette, UpcomingItemView) {
        return Marionette.CollectionView.extend({
            itemView: UpcomingItemView
        });
    });
