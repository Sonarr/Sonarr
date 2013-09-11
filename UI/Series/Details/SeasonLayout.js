'use strict';
define(
    [
        'app',
        'marionette',
        'backgrid',
        'Cells/ToggleCell',
        'Cells/EpisodeTitleCell',
        'Cells/RelativeDateCell',
        'Cells/EpisodeStatusCell',
        'Shared/Actioneer',
        'moment'
    ], function (App, Marionette, Backgrid, ToggleCell, EpisodeTitleCell, RelativeDateCell, EpisodeStatusCell, Actioneer, Moment) {
        return Marionette.Layout.extend({
            template: 'Series/Details/SeasonLayoutTemplate',

            ui: {
                seasonSearch   : '.x-season-search',
                seasonMonitored: '.x-season-monitored',
                seasonRename   : '.x-season-rename'
            },

            events: {
                'click .x-season-search'      : '_seasonSearch',
                'click .x-season-monitored'   : '_seasonMonitored',
                'click .x-season-rename'      : '_seasonRename',
                'click .x-show-hide-episodes' : '_showHideEpisodes',
                'dblclick .series-season h2'  : '_showHideEpisodes'
            },

            regions: {
                episodeGrid: '.x-episode-grid'
            },

            columns:
                [
                    {
                        name      : 'monitored',
                        label     : '',
                        cell      : ToggleCell,
                        trueClass : 'icon-bookmark',
                        falseClass: 'icon-bookmark-empty',
                        tooltip   : 'Toggle monitored status',
                        sortable  : false
                    },
                    {
                        name : 'episodeNumber',
                        label: '#',
                        cell : Backgrid.IntegerCell.extend({
                            className: 'episode-number-cell'
                        })
                    },
                    {
                        name    : 'this',
                        label   : 'Title',
                        hideSeriesLink : true,
                        cell    : EpisodeTitleCell,
                        sortable: false
                    },
                    {
                        name : 'airDateUtc',
                        label: 'Air Date',
                        cell : RelativeDateCell
                    } ,
                    {
                        name    : 'status',
                        label   : 'Status',
                        cell    : EpisodeStatusCell,
                        sortable: false
                    }
                ],

            initialize: function (options) {

                if (!options.episodeCollection) {
                    throw 'episodeCollection is needed';
                }

                this.templateHelpers = {};
                this.episodeCollection = options.episodeCollection.bySeason(this.model.get('seasonNumber'));
                this.series = options.series;

                this.showingEpisodes = this._shouldShowEpisodes();
                this._generateTemplateHelpers();

                this.listenTo(this.model, 'sync', function () {
                    this._afterSeasonMonitored();
                }, this);

                this.listenTo(this.episodeCollection, 'sync', function () {
                    this.render();
                }, this);
            },

            onRender: function () {
                if (this.showingEpisodes) {
                    this._showEpisodes();
                }

                this._setSeasonMonitoredState();
            },

            _seasonSearch: function () {
                Actioneer.ExecuteCommand({
                    command     : 'seasonSearch',
                    properties  : {
                        seriesId    : this.model.get('seriesId'),
                        seasonNumber: this.model.get('seasonNumber')
                    },
                    element       : this.ui.seasonSearch,
                    errorMessage  : 'Search for season {0} failed'.format(this.model.get('seasonNumber')),
                    startMessage  : 'Search for season {0} started'.format(this.model.get('seasonNumber')),
                    successMessage: 'Search for season {0} completed'.format(this.model.get('seasonNumber'))
                });
            },

            _seasonMonitored: function () {
                var name = 'monitored';
                this.model.set(name, !this.model.get(name));
                this.series.setSeasonMonitored(this.model.get('seasonNumber'));

                Actioneer.SaveModel({
                    model  : this.series,
                    context: this,
                    element: this.ui.seasonMonitored,
                    always : this._afterSeasonMonitored
                });
            },

            _afterSeasonMonitored: function () {
                var self = this;

                _.each(this.episodeCollection.models, function (episode) {
                    episode.set({ monitored: self.model.get('monitored') });
                });

                this.render();
            },

            _setSeasonMonitoredState: function () {
                this.ui.seasonMonitored.removeClass('icon-spinner icon-spin');

                if (this.model.get('monitored')) {
                    this.ui.seasonMonitored.addClass('icon-bookmark');
                    this.ui.seasonMonitored.removeClass('icon-bookmark-empty');
                }
                else {
                    this.ui.seasonMonitored.addClass('icon-bookmark-empty');
                    this.ui.seasonMonitored.removeClass('icon-bookmark');
                }
            },

            _seasonRename: function () {
                Actioneer.ExecuteCommand({
                    command     : 'renameSeason',
                    properties  : {
                        seriesId    : this.model.get('seriesId'),
                        seasonNumber: this.model.get('seasonNumber')
                    },
                    element     : this.ui.seasonRename,
                    errorMessage: 'Season rename failed',
                    context     : this,
                    onSuccess   : this._afterRename
                });
            },

            _afterRename: function () {
                App.vent.trigger(App.Events.SeasonRenamed, { series: this.series, seasonNumber: this.model.get('seasonNumber') });
            },

            _showEpisodes: function () {
                this.episodeGrid.show(new Backgrid.Grid({
                    columns   : this.columns,
                    collection: this.episodeCollection,
                    className : 'table table-hover season-grid'
                }));
            },

            _shouldShowEpisodes: function () {
                var startDate = Moment().add('month', -1);
                var endDate = Moment().add('year', 1);

                var result = this.episodeCollection.some(function(episode) {

                    var airDate = episode.get('airDateUtc');

                    if (airDate)
                    {
                        var airDateMoment = Moment(airDate);

                        if (airDateMoment.isAfter(startDate) && airDateMoment.isBefore(endDate)) {
                            return true;
                        }
                    }

                    return false;
                });

                return result;
            },

            _generateTemplateHelpers: function () {
                this.templateHelpers.showingEpisodes = this.showingEpisodes;

                var episodeCount = this.episodeCollection.filter(function (episode) {
                    return (episode.get('monitored')  && Moment(episode.get('airDateUtc')).isBefore(Moment())) || episode.get('hasFile');
                }).length;

                var episodeFileCount = this.episodeCollection.where({ hasFile: true }).length;
                var percentOfEpisodes = 100;

                if (episodeCount > 0) {
                    percentOfEpisodes = episodeFileCount / episodeCount * 100;
                }

                this.templateHelpers.episodeCount = episodeCount;
                this.templateHelpers.episodeFileCount = episodeFileCount;
                this.templateHelpers.percentOfEpisodes = percentOfEpisodes;
            },

            _showHideEpisodes: function () {
                if (this.showingEpisodes) {
                    this.showingEpisodes = false;
                    this.episodeGrid.$el.slideUp();
                    this.episodeGrid.close();
                }

                else {
                    this.showingEpisodes = true;
                    this._showEpisodes();
                    this.episodeGrid.$el.slideDown();
                }

                this.templateHelpers.showingEpisodes = this.showingEpisodes;
                this.render();
            }
        });
    });
