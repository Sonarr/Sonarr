var Marionette = require('marionette');
var MetadataCollection = require('./MetadataCollection');
var MetadataCollectionView = require('./MetadataCollectionView');

module.exports = Marionette.Layout.extend({
    template   : 'Settings/Metadata/MetadataLayoutTemplate',
    regions    : {metadata : '#x-metadata-providers'},
    initialize : function(options){
        this.settings = options.settings;
        this.metadataCollection = new MetadataCollection();
        this.metadataCollection.fetch();
    },
    onShow     : function(){
        this.metadata.show(new MetadataCollectionView({collection : this.metadataCollection}));
    }
});