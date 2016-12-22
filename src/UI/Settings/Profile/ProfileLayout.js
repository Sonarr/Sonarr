var Marionette = require('marionette');
var ProfileCollection = require('../../Profile/ProfileCollection');
var LanguageProfileCollection = require('../../LanguageProfile/LanguageProfileCollection');
var ProfileCollectionView = require('./ProfileCollectionView');
var LanguageProfileCollectionView = require('../LanguageProfile/LanguageProfileCollectionView');
var DelayProfileLayout = require('./Delay/DelayProfileLayout');
var DelayProfileCollection = require('./Delay/DelayProfileCollection');

module.exports = Marionette.Layout.extend({
    template : 'Settings/Profile/ProfileLayoutTemplate',

    regions : {
        profile         : '#profile',
        languageProfile : '#language-profile',
        delayProfile    : '#delay-profile'
    },

    initialize : function(options) {
        this.settings = options.settings;
        ProfileCollection.fetch();
        LanguageProfileCollection.fetch();

        this.delayProfileCollection = new DelayProfileCollection();
        this.delayProfileCollection.fetch();
    },

    onShow : function() {
        this.profile.show(new ProfileCollectionView({ collection : ProfileCollection }));
        this.delayProfile.show(new DelayProfileLayout({ collection : this.delayProfileCollection }));
        this.languageProfile.show(new LanguageProfileCollectionView( {collection : LanguageProfileCollection}));
    }
});