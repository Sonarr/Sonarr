var Backbone = require('backbone');
var MovieFileModel = require('./MovieFileModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/moviefile',
    model : MovieFileModel,

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        this.movieId = options.movieId;
        this.models = [];
    },

    fetch : function(options) {
        if (!this.movieId) {
            throw 'movieId is required';
        }

        if (!options) {
            options = {};
        }

        options.data = { movieId : this.movieId };

        return this.originalFetch.call(this, options);
    }
});