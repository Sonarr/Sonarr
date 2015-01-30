'use strict';

define([
    'marionette',
    'Settings/Indexers/IndexerCollection',
    'Settings/Indexers/IndexerCollectionView',
    'Settings/Indexers/Options/IndexerOptionsView',
    'Settings/Indexers/Restriction/RestrictionCollection',
    'Settings/Indexers/Restriction/RestrictionCollectionView'
], function (Marionette, IndexerCollection, CollectionView, OptionsView, RestrictionCollection, RestrictionCollectionView) {

    return Marionette.Layout.extend({
        template: 'Settings/Indexers/IndexerLayoutTemplate',

        regions: {
            indexers       : '#x-indexers-region',
            indexerOptions : '#x-indexer-options-region',
            restriction    : '#x-restriction-region'
        },

        initialize: function () {
            this.indexersCollection = new IndexerCollection();
            this.indexersCollection.fetch();

            this.restrictionCollection = new RestrictionCollection();
            this.restrictionCollection.fetch();
        },

        onShow: function () {
            this.indexers.show(new CollectionView({ collection: this.indexersCollection }));
            this.indexerOptions.show(new OptionsView({ model: this.model }));
            this.restriction.show(new RestrictionCollectionView({ collection: this.restrictionCollection }));
        }
    });
});
