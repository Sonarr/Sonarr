var Backbone = require('backbone');
var SeriesCollection = require('../../Series/SeriesCollection');
var MovieCollection = require('../../Movie/MovieCollection');

module.exports = Backbone.Model.extend({

    //Hack to deal with Backbone 1.0's bug
    initialize : function() {
        this.url = function() {
            return this.collection.url + '/' + this.get('id');
        };
    },

    parse : function(model) {
        model.series = SeriesCollection.get(model.seriesId);
        model.movie = MovieCollection.get(model.movieId);
        return model;
    }
});