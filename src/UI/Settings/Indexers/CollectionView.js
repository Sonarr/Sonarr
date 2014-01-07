'use strict';
define(
    [
        'AppLayout',
        'marionette',
        'Settings/Indexers/ItemView',
        'Settings/Indexers/EditView',
        'Settings/Indexers/Collection',
        'System/StatusModel',
        'underscore'
    ], function (AppLayout, Marionette, IndexerItemView, IndexerEditView, IndexerCollection, StatusModel, _) {
        return Marionette.CompositeView.extend({
            itemView         : IndexerItemView,
            itemViewContainer: '#x-indexers',
            template         : 'Settings/Indexers/CollectionTemplate',

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
                var self = this;
                //TODO: Is there a better way to deal with changing URLs?
                var schemaCollection = new IndexerCollection();
                schemaCollection.url = StatusModel.get('urlBase') + '/api/indexer/schema';
                schemaCollection.fetch({
                    success: function (collection) {
                        collection.url = './api/indexer';
                        var model = _.first(collection.models);

                        model.set({
                            id    : undefined,
                            name  : '',
                            enable: true
                        });

                        var view = new IndexerEditView({ model: model, indexerCollection: self.collection});
                        AppLayout.modalRegion.show(view);
                    }
                });
            }
        });
    });
