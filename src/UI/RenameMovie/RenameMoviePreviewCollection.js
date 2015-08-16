var Backbone = require('backbone');
var RenameMoviePreviewModel = require('./RenameMoviePreviewModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/renamemovie',
    model : RenameMoviePreviewModel,

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        if (!options.movieId) {
            throw 'movieId is required';
        }

        this.movieId = options.movieId;
    },

    fetch : function(options) {
        if (!this.movieId) {
            throw 'movieId is required';
        }

        options = options || {};
        options.data = {};
        options.data.movieId = this.movieId;

        return this.originalFetch.call(this, options);
    }
});