'use strict';
define(['app',
        'Series/Details/SeasonLayout',
        'Series/SeasonCollection',
        'Series/EpisodeCollection'],
    function (App, SeasonLayout, SeasonCollection, EpisodeCollection) {
    NzbDrone.Series.Details.SeasonCollectionView = Backbone.Marionette.CollectionView.extend({

        itemView         : SeasonLayout,

        initialize: function (options) {

            if (!options.episodeCollection) {
                throw 'episodeCollection is needed';
            }

            this.episodeCollection = options.episodeCollection;

        },

        itemViewOptions: function () {
            return {
                episodeCollection: this.episodeCollection
            };
        }

    });
});
