'use strict';
define(
    [
        'marionette',
        'SeasonPass/SeriesLayout'
    ], function (Marionette, SeriesLayout) {
        return Marionette.CollectionView.extend({

            itemView: SeriesLayout,

            initialize: function (options) {

                if (!options.seasonCollection) {
                    throw 'seasonCollection is needed';
                }

                this.seasonCollection = options.seasonCollection;
            },

            itemViewOptions: function () {
                return {
                    seasonCollection: this.seasonCollection
                };
            }
        });
    });
