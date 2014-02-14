'use strict';

define([
    'marionette',
    'Settings/DownloadClient/Add/DownloadClientAddItemView'
], function (Marionette, AddItemView) {

    return Marionette.CompositeView.extend({
        itemView         : AddItemView,
        itemViewContainer: '.add-download-client .items',
        template         : 'Settings/DownloadClient/Add/DownloadClientAddCollectionViewTemplate',

        itemViewOptions: function () {
            return {
                downloadClientCollection: this.downloadClientCollection
            };
        },

        initialize: function (options) {
            this.downloadClientCollection = options.downloadClientCollection;
        }
    });
});
