var Backbone = require('backbone');
var EpisodeModel = require('../Series/EpisodeModel');

module.exports = Backbone.Collection.extend({
    url       : window.NzbDrone.ApiRoot + '/calendar',
    model     : EpisodeModel,
    tableName : 'calendar',

    comparator : function(model) {
        var date = new Date(model.get('airDateUtc'));
        var time = date.getTime();
        return time;
    }
});