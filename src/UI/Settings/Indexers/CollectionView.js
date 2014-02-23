'use strict';
define(
    [
        'AppLayout',
        'marionette',
        'Settings/Indexers/ItemView',
        'Settings/Indexers/EditView',
        'Settings/Indexers/Collection',
        'underscore'
    ], function (AppLayout, Marionette, IndexerItemView, IndexerEditView, IndexerCollection, _) {
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
                var schemaCollection = new IndexerCollection();
                var originalUrl = schemaCollection.url;

                schemaCollection.url = schemaCollection.url + '/schema';

                schemaCollection.fetch({
                    success: function (collection) {
                        collection.url = originalUrl;
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
