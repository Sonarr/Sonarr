'use strict';
define([
    'app',
    'Cells/EpisodeStatusCell',
    'Cells/EpisodeTitleCell',
    'Cells/AirDateCell',
    'Cells/ToggleCell',
    'Shared/Messenger'],
    function (App, EpisodeStatusCell, EpisodeTitleCell, AirDateCell, ToggleCell, Messenger) {
        return Backbone.Marionette.Layout.extend({
            template: 'Series/Details/SeasonLayoutTemplate',

            ui: {
                seasonSearch: '.x-season-search'
            },

            events: {
                'click .x-season-search': '_seasonSearch'
            },

            regions: {
                episodeGrid: '#x-episode-grid'
            },

            columns: [

                {
                    name      : 'ignored',
                    label     : '',
                    cell      : ToggleCell,
                    trueClass : 'icon-bookmark-empty',
                    falseClass: 'icon-bookmark'
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
            },

            onShow: function () {
                this.episodeGrid.show(new Backgrid.Grid(
                    {
                        columns   : this.columns,
                        collection: this.episodeCollection,
                        className : 'table table-hover season-grid'
                    }));
            },

            _seasonSearch: function () {
                var command = 'seasonSearch';

                this.idle = false;

                this.ui.seasonSearch.addClass('icon-spinner icon-spin');

                var properties = {
                    seriesId: this.model.get('seriesId'),
                    seasonNumber: this.model.get('seasonNumber')
                };

                var self = this;
                var commandPromise = App.Commands.Execute(command, properties);

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
            }
        });
    });
