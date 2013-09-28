'use strict';
define(
    [
        'marionette',
        'Update/UpdateItemView'
    ], function (Marionette, UpdateItemView) {
        return Marionette.CollectionView.extend({
            itemView: UpdateItemView
        });
    });
