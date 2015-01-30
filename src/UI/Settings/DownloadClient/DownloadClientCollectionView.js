'use strict';

define([
    'marionette',
    'Settings/DownloadClient/DownloadClientItemView',
    'Settings/DownloadClient/Add/DownloadClientSchemaModal'
], function (Marionette, ItemView, SchemaModal) {
    return Marionette.CompositeView.extend({
        itemView         : ItemView,
        itemViewContainer: '.download-client-list',
        template         : 'Settings/DownloadClient/DownloadClientCollectionViewTemplate',

        ui: {
            'addCard': '.x-add-card'
        },

        events: {
            'click .x-add-card': '_openSchemaModal'
        },

        appendHtml: function (collectionView, itemView, index) {
            collectionView.ui.addCard.parent('li').before(itemView.el);
        },

        _openSchemaModal: function () {
            SchemaModal.open(this.collection);
        }
    });
});
