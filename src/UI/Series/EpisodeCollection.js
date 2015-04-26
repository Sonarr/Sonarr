var Backbone = require('backbone');
var PageableCollection = require('backbone.pageable');
var EpisodeModel = require('./EpisodeModel');
require('./EpisodeCollection');

module.exports = PageableCollection.extend({
    url   : window.NzbDrone.ApiRoot + '/episode',
    model : EpisodeModel,

    state : {
        sortKey  : 'episodeNumber',
        order    : 1,
        pageSize : 100000
    },

    mode : 'client',

    originalFetch : Backbone.Collection.prototype.fetch,

    initialize : function(options) {
        this.seriesId = options.seriesId;
    },

    bySeason : function(season) {
        var filtered = this.filter(function(episode) {
            return episode.get('seasonNumber') === season;
        });

        var EpisodeCollection = require('./EpisodeCollection');

        return new EpisodeCollection(filtered);
    },

    comparator : function(model1, model2) {
        var episode1 = model1.get('episodeNumber');
        var episode2 = model2.get('episodeNumber');

        if (episode1 < episode2) {
            return 1;
        }

        if (episode1 > episode2) {
            return -1;
        }

        return 0;
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