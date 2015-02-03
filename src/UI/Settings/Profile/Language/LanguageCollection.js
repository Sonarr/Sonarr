var Backbone = require('backbone');
var LanguageModel = require('./LanguageModel');

module.exports = (function(){
    var LanuageCollection = Backbone.Collection.extend({
        model : LanguageModel,
        url   : window.NzbDrone.ApiRoot + '/language'
    });
    var languages = new LanuageCollection();
    languages.fetch();
    return languages;
}).call(this);