'use strict';
define(['app',
    'marionette',
    'Settings/Indexers/ItemView',
    'Settings/Indexers/EditView',
    'Settings/Indexers/Collection'],
    function (App, Marionette, IndexerItemView, IndexerEditView, IndexerCollection) {
        return Marionette.CompositeView.extend({
            itemView         : IndexerItemView,
            itemViewContainer: '#x-indexers',
            template         : 'Settings/Indexers/CollectionTemplate',

            events: {
                'click .x-add': 'openSchemaModal'
            },

            openSchemaModal: function () {
                var self = this;
                //TODO: Is there a better way to deal with changing URLs?
                var schemaCollection = new IndexerCollection();
                schemaCollection.url = '/api/indexer/schema';
                schemaCollection.fetch({
                    success: function (collection) {
                        collection.url = '/api/indexer';
                        var model = _.first(collection.models);
                        model.set('id', undefined);
                        model.set('name', '');

                        var view = new IndexerEditView({ model: model, indexerCollection: self.collection});
                        App.modalRegion.show(view);
                    }
                });
            }
        });
    });
