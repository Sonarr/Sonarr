var _ = require('underscore');
var Marionette = require('marionette');
var SeasonLayout = require('./SeasonLayout');
var AsSortedCollectionView = require('../../Mixins/AsSortedCollectionView');

var view = Marionette.CollectionView.extend({

    itemView : SeasonLayout,

    initialize : function(options) {
        if (!options.episodeCollection) {
            throw 'episodeCollection is needed';
        }

        this.episodeCollection = options.episodeCollection;
        this.series = options.series;
    },

    itemViewOptions : function() {
        return {
            episodeCollection : this.episodeCollection,
            series            : this.series
        };
    },

    onEpisodeGrabbed : function(message) {
        if (message.episode.series.id !== this.episodeCollection.seriesId) {
            return;
        }

        var self = this;

        _.each(message.episode.episodes, function(episode) {
            var ep = self.episodeCollection.get(episode.id);
            ep.set('downloading', true);
        });

        this.render();
    }
});

AsSortedCollectionView.call(view);

module.exports = view;