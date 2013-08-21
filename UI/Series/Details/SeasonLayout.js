'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Cells/ToggleCell',
        'Cells/EpisodeTitleCell',
        'Cells/RelativeDateCell',
        'Cells/EpisodeStatusCell',
        'Commands/CommandController',
        'Shared/Actioneer'
    ], function ( Marionette, Backgrid, ToggleCell, EpisodeTitleCell, RelativeDateCell, EpisodeStatusCell, CommandController, Actioneer) {
        return Marionette.Layout.extend({
            template: 'Series/Details/SeasonLayoutTemplate',

            ui: {
                seasonSearch   : '.x-season-search',
                seasonMonitored: '.x-season-monitored',
                seasonRename   : '.x-season-rename'
            },

            events: {
                'click .x-season-search'   : '_seasonSearch',
                'click .x-season-monitored': '_seasonMonitored',
                'click .x-season-rename'   : '_seasonRename'
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

                this.episodeCollection = options.episodeCollection.bySeason(this.model.get('seasonNumber'));

                this.listenTo(this.model, 'sync', function () {
                    this._afterSeasonMonitored();
                }, this);

                this.listenTo(this.episodeCollection, 'sync', function () {
                    this.render();
                }, this);
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
                Actioneer.ExecuteCommand({
                    command     : 'seasonSearch',
                    properties  : {
                        seriesId    : this.model.get('seriesId'),
                        seasonNumber: this.model.get('seasonNumber')
                    },
                    element     : this.ui.seasonSearch,
                    failMessage : 'Search for season {0} failed'.format(this.model.get('seasonNumber')),
                    startMessage: 'Search for season {0} started'.format(this.model.get('seasonNumber'))
                });
            },

            _seasonMonitored: function () {
                var name = 'monitored';
                this.model.set(name, !this.model.get(name));

                Actioneer.SaveModel({
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
                    command    : 'renameSeason',
                    properties : {
                        seriesId    : this.model.get('seriesId'),
                        seasonNumber: this.model.get('seasonNumber')
                    },
                    element    : this.ui.seasonRename,
                    failMessage: 'Season rename failed'
                });
            }
        });
    });
