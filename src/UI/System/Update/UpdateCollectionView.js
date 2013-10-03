'use strict';
define(
    [
        'marionette',
        'System/Update/UpdateItemView'
    ], function (Marionette, UpdateItemView) {
        return Marionette.CollectionView.extend({
            itemView: UpdateItemView
        });
    });
