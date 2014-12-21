'use strict';

define(
    [
        'marionette',
        'AddSeries/RootFolders/RootFolderItemView'
    ], function (Marionette, RootFolderItemView) {


        return Marionette.CompositeView.extend({
            template          : 'AddSeries/RootFolders/RootFolderCollectionViewTemplate',
            itemViewContainer : '.x-root-folders',
            itemView          : RootFolderItemView
        });
    });
