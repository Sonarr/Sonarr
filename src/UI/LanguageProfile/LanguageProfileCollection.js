var Backbone = require('backbone');
var LanguageProfileModel = require('./LanguageProfileModel');

var LanguageProfileCollection = Backbone.Collection.extend({
    model : LanguageProfileModel,
    url   : window.NzbDrone.ApiRoot + '/languageprofile'
});

var profiles = new LanguageProfileCollection();

profiles.fetch();

module.exports = profiles;
