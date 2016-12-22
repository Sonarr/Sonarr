var Marionette = require('marionette');
var LanguageProfileCollection = require('../../LanguageProfile/LanguageProfileCollection');
var LanguageProfileCollectionView = require('./LanguageProfileCollectionView');


module.exports = Marionette.Layout.extend({
    template : 'Settings/LanguageProfile/LanguageProfileLayoutTemplate',

    regions : {
        profile      : '#profile',
    },

    initialize : function(options) {
        this.settings = options.settings;
        LanguageProfileCollection.fetch();

    },

    onShow : function() {
        this.profile.show(new LanguageProfileCollectionView({ collection : LanguageProfileCollection }));
    }
});