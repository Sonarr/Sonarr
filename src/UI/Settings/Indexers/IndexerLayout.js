var Marionette = require('marionette');
var IndexerCollection = require('./IndexerCollection');
var CollectionView = require('./IndexerCollectionView');
var OptionsView = require('./Options/IndexerOptionsView');
var RestrictionCollection = require('./Restriction/RestrictionCollection');
var RestrictionCollectionView = require('./Restriction/RestrictionCollectionView');

module.exports = Marionette.Layout.extend({
    template : 'Settings/Indexers/IndexerLayoutTemplate',

    regions : {
        indexers       : '#x-indexers-region',
        indexerOptions : '#x-indexer-options-region',
        restriction    : '#x-restriction-region'
    },

    initialize : function() {
        this.indexersCollection = new IndexerCollection();
        this.indexersCollection.fetch();

        this.restrictionCollection = new RestrictionCollection();
        this.restrictionCollection.fetch();
    },

    onShow : function() {
        this.indexers.show(new CollectionView({ collection : this.indexersCollection }));
        this.indexerOptions.show(new OptionsView({ model : this.model }));
        this.restriction.show(new RestrictionCollectionView({ collection : this.restrictionCollection }));
    }
});