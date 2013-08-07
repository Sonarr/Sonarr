'use strict';
define(
    [
        'app',
        'marionette',
        'Series/EpisodeCollection',
        'Series/SeasonCollection',
        'Series/Details/SeasonCollectionView',
        'Series/Details/SeasonMenu/CollectionView',
        'Shared/LoadingView',
        'Shared/Actioneer',
        'backstrech'
    ], function (App, Marionette, EpisodeCollection, SeasonCollection, SeasonCollectionView, SeasonMenuCollectionView, LoadingView, Actioneer) {
        return Marionette.Layout.extend({

            itemViewContainer: '.x-series-seasons',
            template         : 'Series/Details/SeriesDetailsTemplate',

            regions: {
                seasonMenu: '#season-menu',
                seasons   : '#seasons'
            },

            ui: {
                header   : '.x-header',
                monitored: '.x-monitored',
                edit     : '.x-edit',
                refresh  : '.x-refresh',
                rename   : '.x-rename',
                search   : '.x-search'
            },

            events: {
                'click .x-monitored': '_toggleMonitored',
                'click .x-edit'     : '_editSeries',
                'click .x-refresh'  : '_refreshSeries',
                'click .x-rename'   : '_renameSeries',
                'click .x-search'   : '_seriesSearch'
            },

            initialize: function () {
                $('body').addClass('backdrop');

                this.listenTo(this.model, 'sync', function () {
                    this._setMonitoredState()
                }, this);

                this.listenTo(App.vent, App.Events.SeriesDeleted, this._onSeriesDeleted);
            },

            onShow: function () {
                var fanArt = this._getFanArt();

                if (fanArt) {
                    $.backstretch(fanArt);
                }
                else {
                    $('body').removeClass('backdrop');
                }

                this._showSeasons();
                this._setMonitoredState();
            },

            _getFanArt: function () {
                var fanArt = _.where(this.model.get('images'), {coverType: 'fanart'});

                if (fanArt && fanArt[0]) {
                    return fanArt[0].url;
                }

                return undefined;
            },

            onClose: function () {
                $('.backstretch').remove();
                $('body').removeClass('backdrop');
            },

            _toggleMonitored: function () {
                var name = 'monitored';
                this.model.set(name, !this.model.get(name), { silent: true });

                Actioneer.SaveModel({
                    context       : this,
                    element       : this.ui.monitored,
                    alwaysCallback: this._setMonitoredState()
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
                App.vent.trigger(App.Commands.EditSeriesCommand, {series: this.model});
            },

            _refreshSeries: function () {
                Actioneer.ExecuteCommand({
                    command   : 'refreshSeries',
                    properties: {
                        seriesId: this.model.get('id')
                    },
                    element   : this.ui.refresh,
                    leaveIcon : true,
                    context: this,
                    successCallback: this._showSeasons
                });
            },

            _onSeriesDeleted: function (event) {

                if (this.model.get('id') === event.series.get('id')) {
                    App.Router.navigate('/', { trigger: true });
                }
            },

            _renameSeries: function () {
                Actioneer.ExecuteCommand({
                    command    : 'renameSeries',
                    properties : {
                        seriesId: this.model.get('id')
                    },
                    element    : this.ui.rename,
                    failMessage: 'Series search failed'
                });
            },

            _seriesSearch: function () {
                Actioneer.ExecuteCommand({
                    command     : 'seriesSearch',
                    properties  : {
                        seriesId: this.model.get('id')
                    },
                    element     : this.ui.search,
                    failMessage : 'Series search failed',
                    startMessage: 'Search for {0} started'.format(this.model.get('title'))
                });
            },

            _showSeasons: function () {
                var self = this;

                this.seasons.show(new LoadingView());

                this.seasonCollection = new SeasonCollection();
                this.episodeCollection = new EpisodeCollection();

                $.when(this.episodeCollection.fetch({data: { seriesId: this.model.id }}), this.seasonCollection.fetch({data: { seriesId: this.model.id }})).done(function () {
                    self.seasons.show(new SeasonCollectionView({
                        collection       : self.seasonCollection,
                        episodeCollection: self.episodeCollection,
                        series           : self.model
                    }));

                    self.seasonMenu.show(new SeasonMenuCollectionView({
                        collection: self.seasonCollection,
                        episodeCollection: self.episodeCollection
                    }));
                });
            }
        });
    });
