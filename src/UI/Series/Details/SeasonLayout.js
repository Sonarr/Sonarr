'use strict';
define(
    [
        'vent',
        'marionette',
        'backgrid',
        'Cells/ToggleCell',
        'Cells/EpisodeTitleCell',
        'Cells/RelativeDateCell',
        'Cells/EpisodeStatusCell',
        'Cells/EpisodeActionsCell',
        'Commands/CommandController',
        'moment',
        'underscore'
    ], function (vent, Marionette, Backgrid, ToggleCell, EpisodeTitleCell, RelativeDateCell, EpisodeStatusCell, EpisodeActionsCell, CommandController, Moment,_) {
        return Marionette.Layout.extend({
            template: 'Series/Details/SeasonLayoutTemplate',

            ui: {
                seasonSearch   : '.x-season-search',
                seasonMonitored: '.x-season-monitored',
                seasonRename   : '.x-season-rename'
            },

            events: {
                'click .x-season-monitored'  : '_seasonMonitored',
                'click .x-season-search'     : '_seasonSearch',
                'click .x-season-rename'     : '_seasonRename',
                'click .x-show-hide-episodes': '_showHideEpisodes',
                'dblclick .series-season h2' : '_showHideEpisodes'
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
                        name          : 'this',
                        label         : 'Title',
                        hideSeriesLink: true,
                        cell          : EpisodeTitleCell,
                        sortable      : false
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
                    },
                    {
                        name    : 'this',
                        label   : '',
                        cell    : EpisodeActionsCell,
                        sortable: false
                    }
                ],

            initialize: function (options) {

                if (!options.episodeCollection) {
                    throw 'episodeCollection is needed';
                }

                this.episodeCollection = options.episodeCollection.bySeason(this.model.get('seasonNumber'));
                this.series = options.series;

                this.showingEpisodes = this._shouldShowEpisodes();

                this.listenTo(this.model, 'sync', this._afterSeasonMonitored);
                this.listenTo(this.episodeCollection, 'sync', this.render);
            },

            onRender: function () {


                if (this.showingEpisodes) {
                    this._showEpisodes();
                }

                this._setSeasonMonitoredState();

                CommandController.bindToCommand({
                    element: this.ui.seasonSearch,
                    command: {
                        name        : 'seasonSearch',
                        seriesId    : this.series.id,
                        seasonNumber: this.model.get('seasonNumber')
                    }
                });

                CommandController.bindToCommand({
                    element: this.ui.seasonRename,
                    command: {
                        name        : 'renameSeason',
                        seriesId    : this.series.id,
                        seasonNumber: this.model.get('seasonNumber')
                    }
                });
            },

            _seasonSearch: function () {

                CommandController.Execute('seasonSearch', {
                    name        : 'seasonSearch',
                    seriesId    : this.series.id,
                    seasonNumber: this.model.get('seasonNumber')
                });
            },

            _seasonRename: function () {

                CommandController.Execute('renameSeason', {
                    name        : 'renameSeason',
                    seriesId    : this.series.id,
                    seasonNumber: this.model.get('seasonNumber')
                });
            },

            _seasonMonitored: function () {
                var name = 'monitored';
                this.model.set(name, !this.model.get(name));
                this.series.setSeasonMonitored(this.model.get('seasonNumber'));

                var savePromise = this.series.save().always(this._afterSeasonMonitored.bind(this));

                this.ui.seasonMonitored.spinForPromise(savePromise);
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


            _afterRename: function () {
                vent.trigger(vent.Events.SeasonRenamed, { series: this.series, seasonNumber: this.model.get('seasonNumber') });
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

                return this.episodeCollection.some(function (episode) {

                    var airDate = episode.get('airDateUtc');

                    if (airDate) {
                        var airDateMoment = Moment(airDate);

                        if (airDateMoment.isAfter(startDate) && airDateMoment.isBefore(endDate)) {
                            return true;
                        }
                    }

                    return false;
                });
            },

            templateHelpers: function () {

                var episodeCount = this.episodeCollection.filter(function (episode) {
                    return episode.get('hasFile') || (episode.get('monitored') && Moment(episode.get('airDateUtc')).isBefore(Moment()));
                }).length;

                var episodeFileCount = this.episodeCollection.where({ hasFile: true }).length;
                var percentOfEpisodes = 100;

                if (episodeCount > 0) {
                    percentOfEpisodes = episodeFileCount / episodeCount * 100;
                }

                return {
                    showingEpisodes  : this.showingEpisodes,
                    episodeCount     : episodeCount,
                    episodeFileCount : episodeFileCount,
                    percentOfEpisodes: percentOfEpisodes
                };
            },

            _showHideEpisodes: function () {
                if (this.showingEpisodes) {
                    this.showingEpisodes = false;
                    this.episodeGrid.close();
                }
                else {
                    this.showingEpisodes = true;
                    this._showEpisodes();
                }

                this.templateHelpers.showingEpisodes = this.showingEpisodes;
                this.render();
            }
        });
    });
