"use strict";

define(
    [
        'marionette',
        'AddSeries/RootFolders/ItemView'
    ], function (Marionette, RootFolderItemView) {


        return Marionette.CollectionView.extend({
            itemView: RootFolderItemView,

            tagName  : 'table',
            className: 'table table-hover'
        });
    });
