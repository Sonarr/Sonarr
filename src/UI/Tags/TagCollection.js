var Backbone = require('backbone');
var TagModel = require('./TagModel');
var ApiData = require('../Shared/ApiData');

require('../Mixins/backbone.signalr.mixin');

var collection = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/tag',
    model : TagModel,

    comparator : function(model){
        return model.get('label');
    }
});

module.exports = new collection(ApiData.get('tag')).bindSignalR();
