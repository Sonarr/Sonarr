'use strict';
define(
    [
        'app',
        'marionette',
        'Series/EpisodeCollection',
        'Series/SeasonCollection',
        'Series/Details/SeasonCollectionView',
        'Series/Edit/EditSeriesView',
        'Shared/LoadingView',
        'Commands/CommandController',
        'backstrech'
    ], function (App, Marionette, EpisodeCollection, SeasonCollection, SeasonCollectionView, EditSeriesView, LoadingView, CommandController) {
        return Marionette.Layout.extend({

            itemViewContainer: '.x-series-seasons',
            template         : 'Series/Details/SeriesDetailsTemplate',

            regions: {
                seasons: '#seasons'
            },

            ui: {
                header   : '.x-header',
                monitored: '.x-monitored',
                edit     : '.x-edit',
                refresh  : '.x-refresh'
            },

            events: {
                'click .x-monitored': '_toggleMonitored',
                'click .x-edit'     : '_editSeries',
                'click .x-refresh'  : '_refreshSeries'
            },

            initialize: function () {
                $('body').addClass('backdrop');

                this.model.on('sync', function () {
                    this._setMonitoredState()
                }, this);

                this.listenTo(App.vent, App.Events.SeriesDeleted, this._onSeriesDeleted);
            },

            onShow: function () {
                var self = this;

                var fanArt = this._getFanArt();

                if (fanArt) {
                    $.backstretch(fanArt);
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
                        episodeCollection: self.episodeCollection,
                        series           : self.model
                    }));
                });

                this._setMonitoredState();
            },

            _getFanArt: function () {
                var fanArt = _.where(this.model.get('images'), {coverType: 'fanart'});

                if(fanArt && fanArt[0]){
                    return fanArt[0].url;
                }

                return undefined;
            },

            onClose: function () {
                $('.backstretch').remove();
                $('body').removeClass('backdrop');
            },

            _toggleMonitored: function () {
                var self = this;
                var name = 'monitored';
                this.model.set(name, !this.model.get(name), { silent: true });

                this.ui.monitored.addClass('icon-spinner icon-spin');

                var promise = this.model.save();

                promise.always(function () {
                    self._setMonitoredState();
                });
            },

            _setMonitoredState: function () {
                var monitored = this.model.get('monitored');

                this.ui.monitored.removeClass('icon-spin icon-spinner');

                if (this.model.get('monitored')) {
                    this.ui.monitored.addClass('icon-bookmark');
                    this.ui.monitored.removeClass('icon-bookmark-empty');
                }
                else {
                    this.ui.monitored.addClass('icon-bookmark-empty');
                    this.ui.monitored.removeClass('icon-bookmark');
                }
            },

            _editSeries: function () {
                var view = new EditSeriesView({ model: this.model });
                App.modalRegion.show(view);
            },

            _refreshSeries: function () {
                var self = this;

                this.ui.refresh.addClass('icon-spin');
                var promise = CommandController.Execute('refreshseries', { seriesId: this.model.get('id') });

                promise.always(function () {
                    self.ui.refresh.removeClass('icon-spin');
                });
            },

            _onSeriesDeleted: function (event) {

                if (this.model.get('id') === event.series.get('id')) {
                    App.Router.navigate('/', { trigger: true });
                }
            }
        });
    });
