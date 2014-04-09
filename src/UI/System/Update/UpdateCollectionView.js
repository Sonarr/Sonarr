'use strict';
define(
    [
        'marionette',
        'System/Update/UpdateItemView',
        'System/Update/EmptyView'
    ], function (Marionette, UpdateItemView, EmptyView) {
        return Marionette.CollectionView.extend({
            itemView : UpdateItemView,
            emptyView: EmptyView
        });
    });
