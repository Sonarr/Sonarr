﻿'use strict';

define(
    [
        'marionette',
        'Series/Index/List/ItemView'
    ], function (Marionette, ListItemView) {

        return Marionette.CompositeView.extend({
            itemView         : ListItemView,
            itemViewContainer: '#x-series-list',
            template         : 'Series/Index/List/CollectionTemplate'
        });
    });
