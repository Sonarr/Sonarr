var Backbone = require('backbone');
var TagModel = require('./TagModel');
var ApiData = require('../Shared/ApiData');

module.exports = (function(){
    var Collection = Backbone.Collection.extend({
        url   : window.NzbDrone.ApiRoot + '/tag',
        model : TagModel
    });
    return new Collection(ApiData.get('tag'));
}).call(this);