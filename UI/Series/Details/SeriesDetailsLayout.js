"use strict";
define(['app', 'Series/Details/SeasonCollectionView', 'Shared/LoadingView'], function () {
        NzbDrone.Series.Details.SeriesDetailsLayout = Backbone.Marionette.Layout.extend({

            itemViewContainer: '.x-series-seasons',
            template         : 'Series/Details/SeriesDetailsTemplate',

            regions: {
                seasons: '#seasons'
            },

            ui: {
                header: '.x-header'
            },

            initialize: function () {
                $('body').addClass('backdrop');
            },

            onShow: function () {
                var self = this;

                $.backstretch(this.model.get('fanArt'));

                this.seasons.show(new NzbDrone.Shared.LoadingView());

                this.seasonCollection = new NzbDrone.Series.SeasonCollection();
                this.episodeCollection = new NzbDrone.Series.EpisodeCollection();

                $.when(this.episodeCollection.fetch({data: { seriesId: this.model.id }}), this.seasonCollection.fetch({data: { seriesId: this.model.id }}))
                    .done(function () {
                        self.seasons.show(new NzbDrone.Series.Details.SeasonCollectionView({
                            collection       : self.seasonCollection,
                            episodeCollection: self.episodeCollection
                        }));
                    }
                );
            },

            onClose: function () {
                $('.backstretch').remove();
                $('body').removeClass('backdrop');
            }
        });
    }
);
