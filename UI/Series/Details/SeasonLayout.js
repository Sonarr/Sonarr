'use strict';
define(['app', 'Series/Details/EpisodeStatusCell', 'Series/Details/EpisodeTitleCell'], function () {
    NzbDrone.Series.Details.SeasonLayout = Backbone.Marionette.Layout.extend({
        template: 'Series/Details/SeasonLayoutTemplate',

        regions: {
            episodeGrid: '#x-episode-grid'
        },

        columns: [

            {
                name : 'episodeNumber',
                label: '#',
                cell : Backgrid.IntegerCell.extend({
                    className: 'episode-number-cell'
                })
            },

            {
                name : 'title',
                label: 'Title',
                cell : NzbDrone.Series.Details.EpisodeTitleCell
            },
            {
                name : 'airDate',
                label: 'Air Date',
                cell : Backgrid.DateCell.extend({
                    className: 'episode-air-date-cell'
                })
            } ,
            {
                name : 'status',
                label: 'Status',
                cell : NzbDrone.Series.Details.EpisodeStatusCell
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
                    className : 'table table-hover'
                }));
        }
    });
});
