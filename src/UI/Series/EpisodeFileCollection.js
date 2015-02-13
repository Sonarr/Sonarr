var Backbone = require('backbone');
var EpisodeFileModel = require('./EpisodeFileModel');

module.exports = Backbone.Collection.extend({
    url   : window.NzbDrone.ApiRoot + '/episodefile',
    model : EpisodeFileModel,

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        this.seriesId = options.seriesId;
        this.models = [];
    },

    fetch : function(options) {
        if (!this.seriesId) {
            throw 'seriesId is required';
        }

        if (!options) {
            options = {};
        }

        options.data = { seriesId : this.seriesId };

        return this.originalFetch.call(this, options);
    }
});