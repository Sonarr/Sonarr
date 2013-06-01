"use strict";
define(['app',  'Series/Details/SeasonLayout', 'Series/SeasonCollection', 'Series/EpisodeCollection'], function () {
    NzbDrone.Series.Details.SeasonCollectionView = Backbone.Marionette.CollectionView.extend({

        itemView         : NzbDrone.Series.Details.SeasonLayout,

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
