'use strict';
define(
    [
        'underscore',
        'AppLayout',
        'marionette',
        'Settings/DownloadClient/DownloadClientItemView',
        'Settings/DownloadClient/Add/SchemaModal'
    ], function (_, AppLayout, Marionette, DownloadClientItemView, SchemaModal) {
        return Marionette.CompositeView.extend({
            itemView         : DownloadClientItemView,
            itemViewContainer: '#x-download-clients',
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
