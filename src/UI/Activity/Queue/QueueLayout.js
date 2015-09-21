var Marionette = require('marionette');
var Backgrid = require('backgrid');
var QueueCollection = require('./QueueCollection');
var SeriesTitleCell = require('../../Cells/SeriesTitleCell');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var EpisodeNumberCell = require('../../Cells/EpisodeNumberCell');
var EpisodeTitleCell = require('../../Cells/EpisodeTitleCell');
var QualityCell = require('../../Cells/QualityCell');
var QueueStatusCell = require('./QueueStatusCell');
var QueueActionsCell = require('./QueueActionsCell');
var TimeleftCell = require('./TimeleftCell');
var ProgressCell = require('./ProgressCell');
var ProtocolCell = require('../../Release/ProtocolCell');
var GridPager = require('../../Shared/Grid/Pager');

module.exports = Marionette.Layout.extend({
    template : 'Activity/Queue/QueueLayoutTemplate',

    regions : {
        table      : '#x-queue',
        tableMovie : '#x-queue-movie',
        pager      : '#x-queue-pager',
        pagerMovie : '#x-queue-movie-pager',
    },

    columns : [
        {
            name      : 'status',
            label     : '',
            cell      : QueueStatusCell,
            cellValue : 'this'
        },
        {
            name     : 'series',
            label    : 'Series',
            cell     : SeriesTitleCell
        },
        {
            name     : 'episode',
            label    : 'Episode',
            cell     : EpisodeNumberCell
        },
        {
            name      : 'episodeTitle',
            label     : 'Episode Title',
            cell      : EpisodeTitleCell,
            cellValue : 'episode'
        },
        {
            name     : 'quality',
            label    : 'Quality',
            cell     : QualityCell,
            sortable : false
        },
        {
            name  : 'protocol',
            label : 'Protocol',
            cell  : ProtocolCell
        },
        {
            name      : 'timeleft',
            label     : 'Timeleft',
            cell      : TimeleftCell,
            cellValue : 'this'
        },
        {
            name      : 'episode',
            label     : 'Progress',
            cell      : ProgressCell,
            cellValue : 'this'
        },
        {
            name      : 'status',
            label     : '',
            cell      : QueueActionsCell,
            cellValue : 'this'
        }
    ],

    columnsMovie : [
        {
            name      : 'status',
            label     : '',
            cell      : QueueStatusCell,
            cellValue : 'this'
        },
        {
            name     : 'movie',
            label    : 'Movie',
            cell     : MovieTitleCell
        },
        {
            name     : 'quality',
            label    : 'Quality',
            cell     : QualityCell,
            sortable : false
        },
        {
            name  : 'protocol',
            label : 'Protocol',
            cell  : ProtocolCell
        },
        {
            name      : 'timeleft',
            label     : 'Timeleft',
            cell      : TimeleftCell,
            cellValue : 'this'
        },
        {
            name      : 'episode',
            label     : 'Progress',
            cell      : ProgressCell,
            cellValue : 'this'
        },
        {
            name      : 'status',
            label     : '',
            cell      : QueueActionsCell,
            cellValue : 'this'
        }
    ],

    initialize : function() {
        this.listenTo(QueueCollection, 'sync', this._showTable);
        this.listenTo(QueueCollection, 'sync', this._showTableMovie);
    },

    onShow : function() {
        this._showTable();
        this._showTableMovie();
    },

    _showTableMovie : function() {
        var collection = QueueCollection.getMovies();
        this.tableMovie.show(new Backgrid.Grid({
            columns    : this.columnsMovie,
            collection : collection,
            className  : 'table table-hover'
        }));

        this.pagerMovie.show(new GridPager({
            columns    : this.columnsMovie,
            collection : collection,
        }));
    },

    _showTable : function() {
        var collection = QueueCollection.getSeries();
        this.table.show(new Backgrid.Grid({
            columns    : this.columns,
            collection : collection,
            className  : 'table table-hover'
        }));

        this.pager.show(new GridPager({
            columns    : this.columns,
            collection : collection,
        }));
    }
});
