var Backbone = require('backbone');
var SeriesCollection = require('../../Series/SeriesCollection');

module.exports = Backbone.Model.extend({
    initialize : function() {
        this.url = function() {
            return this.collection.url + '/' + this.get('id');
        };
    },

    parse : function(model) {
        model.series = SeriesCollection.get(model.seriesId);
        return model;
    }
});