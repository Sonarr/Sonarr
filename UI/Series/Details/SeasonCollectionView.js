'use strict';
define(
    [
        'marionette',
        'Series/Details/SeasonLayout'
    ], function (Marionette, SeasonLayout) {
        return Marionette.CollectionView.extend({

            itemView: SeasonLayout,

            initialize: function (options) {

                if (!options.episodeCollection) {
                    throw 'episodeCollection is needed';
                }

                this.episodeCollection = options.episodeCollection;
                this.series = options.series;
            },

            itemViewOptions: function () {
                return {
                    episodeCollection: this.episodeCollection,
                    series           : this.series
                };
            }

        });
    });
