'use strict';
define([
    'app',
    'Cells/EpisodeStatusCell',
    'Cells/EpisodeTitleCell',
    'Cells/AirDateCell',
    'Cells/ToggleCell'],
    function (App, EpisodeStatusCell, EpisodeTitleCell, AirDateCell, ToggleCell) {
        return Backbone.Marionette.Layout.extend({
            template: 'Series/Details/SeasonLayoutTemplate',

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
            }
        });
    });
