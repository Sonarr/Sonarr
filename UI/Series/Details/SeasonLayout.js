'use strict';
define([
    'app',
    'Cells/EpisodeStatusCell',
    'Cells/EpisodeTitleCell',
    'Cells/AirDateCell',
    'Cells/ToggleCell'],
    function () {
        NzbDrone.Series.Details.SeasonLayout = Backbone.Marionette.Layout.extend({
            template: 'Series/Details/SeasonLayoutTemplate',

            regions: {
                episodeGrid: '#x-episode-grid'
            },

            columns: [

                {
                    name      : 'ignored',
                    label     : '',
                    cell      : NzbDrone.Cells.ToggleCell,
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
                    cell : NzbDrone.Cells.EpisodeTitleCell
                },
                {
                    name : 'airDate',
                    label: 'Air Date',
                    cell : NzbDrone.Cells.AirDateCell
                } ,
                {
                    name : 'status',
                    label: 'Status',
                    cell : NzbDrone.Cells.EpisodeStatusCell
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
