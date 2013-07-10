'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Cells/ToggleCell',
        'Cells/EpisodeTitleCell',
        'Cells/AirDateCell',
        'Cells/EpisodeStatusCell',
        'Commands/CommandController',
        'Shared/Messenger'
    ], function ( Marionette, Backgrid, ToggleCell, EpisodeTitleCell, AirDateCell, EpisodeStatusCell, CommandController, Messenger) {
        return Marionette.Layout.extend({
            template: 'Series/Details/SeasonLayoutTemplate',

            ui: {
                seasonSearch   : '.x-season-search',
                seasonMonitored: '.x-season-monitored'
            },

            events: {
                'click .x-season-search'   : '_seasonSearch',
                'click .x-season-monitored': '_seasonMonitored'
            },

            regions: {
                episodeGrid: '#x-episode-grid'
            },

            columns:
                [
                    {
                        name      : 'monitored',
                        label     : '',
                        cell      : ToggleCell,
                        trueClass : 'icon-bookmark',
                        falseClass: 'icon-bookmark-empty',
                        tooltip   : 'Toggle monitored status'
                    },
                    {
                        name : 'episodeNumber',
                        label: '#',
                        cell : Backgrid.IntegerCell.extend({
                            className: 'episode-number-cell'
                        })
                    },
                    {
                        name : 'this',
                        label: 'Title',
                        cell : EpisodeTitleCell
                    },
                    {
                        name : 'airDate',
                        label: 'Air Date',
                        cell : AirDateCell
                    } ,
                    {
                        name : 'status',
                        label: 'Status',
                        cell : EpisodeStatusCell
                    }
                ],

            initialize: function (options) {

                if (!options.episodeCollection) {
                    throw 'episodeCollection is needed';
                }

                this.episodeCollection = options.episodeCollection.bySeason(this.model.get('seasonNumber'));

                _.each(this.episodeCollection.models, function (episode) {
                    episode.set({ hideSeriesLink: true, series: options.series });
                });
            },

            onRender: function () {
                this.episodeGrid.show(new Backgrid.Grid({
                    columns   : this.columns,
                    collection: this.episodeCollection,
                    className : 'table table-hover season-grid'
                }));

                this._setSeasonMonitoredState();
            },

            _seasonSearch: function () {
                var command = 'seasonSearch';

                this.idle = false;

                this.ui.seasonSearch.addClass('icon-spinner icon-spin');

                var properties = {
                    seriesId    : this.model.get('seriesId'),
                    seasonNumber: this.model.get('seasonNumber')
                };

                var self = this;
                var commandPromise = CommandController.Execute(command, properties);

                commandPromise.fail(function (options) {
                    if (options.readyState === 0 || options.status === 0) {
                        return;
                    }

                    Messenger.show({
                        message: 'Season search failed',
                        type   : 'error'
                    });
                });

                commandPromise.always(function () {
                    if (!self.isClosed) {
                        self.ui.seasonSearch.removeClass('icon-spinner icon-spin');
                        self.idle = true;
                    }
                });
            },

            _seasonMonitored: function () {
                var self = this;
                var name = 'monitored';
                this.model.set(name, !this.model.get(name));

                this.ui.seasonMonitored.addClass('icon-spinner icon-spin');

                var promise = this.model.save();

                promise.always(function (){
                    _.each(self.episodeCollection.models, function (episode) {
                        episode.set({ monitored: !episode.get('monitored') });
                    });

                    self.render();
                });
            },

            _setSeasonMonitoredState: function () {
                var monitored = this.model.get('monitored');

                this.ui.seasonMonitored.removeClass('icon-spinner icon-spin');

                if (this.model.get('monitored')) {
                    this.ui.seasonMonitored.addClass('icon-bookmark');
                    this.ui.seasonMonitored.removeClass('icon-bookmark-empty');
                }
                else {
                    this.ui.seasonMonitored.addClass('icon-bookmark-empty');
                    this.ui.seasonMonitored.removeClass('icon-bookmark');
                }
            }
        });
    });
