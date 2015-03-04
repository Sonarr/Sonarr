var _ = require('underscore');
var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var EpisodeCollection = require('../../Series/EpisodeCollection');
var LoadingView = require('../../Shared/LoadingView');
var SelectAllCell = require('../../Cells/SelectAllCell');
var EpisodeNumberCell = require('../../Series/Details/EpisodeNumberCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var SelectEpisodeRow = require('./SelectEpisodeRow');

module.exports = Marionette.Layout.extend({
    template  : 'ManualImport/Episode/SelectEpisodeLayoutTemplate',

    regions : {
        episodes : '.x-episodes'
    },

    events : {
        'click .x-select' : '_selectEpisodes'
    },

    columns : [
        {
            name       : '',
            cell       : SelectAllCell,
            headerCell : 'select-all',
            sortable   : false
        },
        {
            name  : 'episodeNumber',
            label : '#',
            cell  : EpisodeNumberCell
        },
        {
            name           : 'title',
            label          : 'Title',
            hideSeriesLink : true,
            cell           : 'string',
            sortable       : false
        },
        {
            name  : 'airDateUtc',
            label : 'Air Date',
            cell  : RelativeDateCell
        }
    ],

    initialize : function(options) {
        this.series = options.series;
        this.seasonNumber = options.seasonNumber;
    },

    onRender : function() {
        this.episodes.show(new LoadingView());

        this.episodeCollection = new EpisodeCollection({ seriesId : this.series.id });
        this.episodeCollection.fetch();

        this.listenToOnce(this.episodeCollection, 'sync', function () {

            this.episodeView = new Backgrid.Grid({
                columns    : this.columns,
                collection : this.episodeCollection.bySeason(this.seasonNumber),
                className  : 'table table-hover season-grid',
                row        : SelectEpisodeRow
            });

            this.episodes.show(this.episodeView);
        });
    },

    _selectEpisodes : function () {
        var episodes = _.map(this.episodeView.getSelectedModels(), function (episode) {
            return episode.toJSON();
        });

        this.trigger('manualimport:selected:episodes', { episodes: episodes });
        vent.trigger(vent.Commands.CloseModal2Command);
    }
});
