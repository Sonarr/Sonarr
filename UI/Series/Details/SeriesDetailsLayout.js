'use strict';
define(
    [
        'marionette',
        'Series/EpisodeCollection',
        'Series/SeasonCollection',
        'Series/Details/SeasonCollectionView',
        'Shared/LoadingView',
        'backstrech'
    ], function (Marionette, EpisodeCollection, SeasonCollection, SeasonCollectionView, LoadingView) {
        return Marionette.Layout.extend({

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

                if (this.model.has('fanArt')) {
                    $.backstretch(this.model.get('fanArt'));
                }
                else {
                    $('body').removeClass('backdrop');
                }

                this.seasons.show(new LoadingView());

                this.seasonCollection = new SeasonCollection();
                this.episodeCollection = new EpisodeCollection();

                $.when(this.episodeCollection.fetch({data: { seriesId: this.model.id }}), this.seasonCollection.fetch({data: { seriesId: this.model.id }})).done(function () {
                    self.seasons.show(new SeasonCollectionView({
                        collection       : self.seasonCollection,
                        episodeCollection: self.episodeCollection
                    }));
                });
            },

            onClose: function () {
                $('.backstretch').remove();
                $('body').removeClass('backdrop');
            }
        });
    });
