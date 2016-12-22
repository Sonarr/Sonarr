var Backbone = require('backbone');
var ProfileModel = require('../../LanguageProfile/LanguageProfileModel');

module.exports = Backbone.Collection.extend({
    model : ProfileModel,
    url   : window.NzbDrone.ApiRoot + '/languageprofile/schema'
});