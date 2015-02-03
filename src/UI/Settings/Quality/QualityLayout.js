var Marionette = require('marionette');
var QualityDefinitionCollection = require('../../Quality/QualityDefinitionCollection');
var QualityDefinitionCollectionView = require('./Definition/QualityDefinitionCollectionView');

module.exports = Marionette.Layout.extend({
    template   : 'Settings/Quality/QualityLayoutTemplate',
    regions    : {qualityDefinition : '#quality-definition'},
    initialize : function(options){
        this.settings = options.settings;
        this.qualityDefinitionCollection = new QualityDefinitionCollection();
        this.qualityDefinitionCollection.fetch();
    },
    onShow     : function(){
        this.qualityDefinition.show(new QualityDefinitionCollectionView({collection : this.qualityDefinitionCollection}));
    }
});