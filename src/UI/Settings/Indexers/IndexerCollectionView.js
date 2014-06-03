'use strict';

define([
    'marionette',
    'Settings/Indexers/IndexerItemView',
    'Settings/Indexers/Add/IndexerSchemaModal'
], function (Marionette, ItemView, SchemaModal) {
    return Marionette.CompositeView.extend({
        itemView         : ItemView,
        itemViewContainer: '.indexer-list',
        template         : 'Settings/Indexers/IndexerCollectionViewTemplate',

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
