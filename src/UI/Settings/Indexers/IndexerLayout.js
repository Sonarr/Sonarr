'use strict';

define([
    'marionette',
    'Settings/Indexers/IndexerCollection',
    'Settings/Indexers/IndexerCollectionView',
    'Settings/Indexers/Options/IndexerOptionsView'
], function (Marionette, IndexerCollection, CollectionView, OptionsView) {

    return Marionette.Layout.extend({
        template: 'Settings/Indexers/IndexerLayoutTemplate',

        regions: {
            indexers               : '#x-indexers-region',
            indexerOptions         : '#x-indexer-options-region'
        },

        initialize: function (options) {
            this.indexersCollection = new IndexerCollection();
            this.indexersCollection.fetch();
        },

        onShow: function () {
            this.indexers.show(new CollectionView({ collection: this.indexersCollection }));
            this.indexerOptions.show(new OptionsView({ model: this.model }));
        }
    });
});
