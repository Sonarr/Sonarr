var Backbone = require('backbone');
var TagModel = require('./TagModel');
var ApiData = require('../Shared/ApiData');

var Collection = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/tag',
    model : TagModel,

    comparator : function(model) {
        return model.get('label');
    }
});

module.exports = new Collection(ApiData.get('tag'));
