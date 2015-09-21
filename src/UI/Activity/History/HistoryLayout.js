var Marionette = require('marionette');
var Backgrid = require('backgrid');
var HistoryCollection = require('./HistoryCollection');
var EventTypeCell = require('../../Cells/EventTypeCell');
var SeriesTitleCell = require('../../Cells/SeriesTitleCell');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var DetailsMovieCell = require('../../Cells/OriginalTitleMovieCell');
var EpisodeNumberCell = require('../../Cells/EpisodeNumberCell');
var EpisodeTitleCell = require('../../Cells/EpisodeTitleCell');
var HistoryQualityCell = require('./HistoryQualityCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var HistoryDetailsCell = require('./HistoryDetailsCell');
var GridPager = require('../../Shared/Grid/Pager');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
var LoadingView = require('../../Shared/LoadingView');

module.exports = Marionette.Layout.extend({
    template : 'Activity/History/HistoryLayoutTemplate',

    regions : {
        history      : '#x-history',
        historyMovie : '#x-history-movie',
        toolbar      : '#x-history-toolbar',
        toolbarMovie : '#x-history-movie-toolbar',
        pager        : '#x-history-pager',
        pagerMovie   : '#x-history-movie-pager'
    },

    columns : [
        {
            name      : 'eventType',
            label     : '',
            cell      : EventTypeCell,
            cellValue : 'this'
        },
        {
            name  : 'series',
            label : 'Series',
            cell  : SeriesTitleCell,
        },
        {
            name     : 'episode',
            label    : 'Episode',
            cell     : EpisodeNumberCell,
            sortable : false
        },
        {
            name     : 'episode',
            label    : 'Episode Title',
            cell     : EpisodeTitleCell,
            sortable : false
        },
        {
            name     : 'this',
            label    : 'Quality',
            cell     : HistoryQualityCell,
            sortable : false
        },
        {
            name  : 'date',
            label : 'Date',
            cell  : RelativeDateCell
        },
        {
            name     : 'this',
            label    : '',
            cell     : HistoryDetailsCell,
            sortable : false
        }
    ],

    columnsMovie : [
        {
            name      : 'eventType',
            label     : '',
            cell      : EventTypeCell,
            cellValue : 'this'
        },
        {
            name  : 'movie',
            label : 'Movie',
            cell  : MovieTitleCell,
        },
        {
            name     : 'movie',
            label    : 'Original Title',
            cell     : DetailsMovieCell,
            sortable : false
        },
        {
            name     : 'this',
            label    : 'Quality',
            cell     : HistoryQualityCell,
            sortable : false
        },
        {
            name  : 'date',
            label : 'Date',
            cell  : RelativeDateCell
        },
        {
            name     : 'this',
            label    : '',
            cell     : HistoryDetailsCell,
            sortable : false
        }
    ],

    initialize : function() {
        this.collection = new HistoryCollection({ tableName : 'history',
                                                  mediaType : 1 });
        this.movieCollection = new HistoryCollection({ tableName : 'history',
                                                       mediaType : 2 });
        this.listenTo(this.collection, 'sync', this._showTable);
        this.listenTo(this.movieCollection, 'sync', this._showTableMovie);
    },

    onShow : function() {
        this.history.show(new LoadingView());
        this.historyMovie.show(new LoadingView());
        this._showToolbar();
        this._showToolbarMovie();
    },

    _showTableMovie : function(collection) {
        this.historyMovie.show(new Backgrid.Grid({
            columns    : this.columnsMovie,
            collection : collection,
            className  : 'table table-hover'
        }));

        this.pagerMovie.show(new GridPager({
            columns    : this.columnsMovie,
            collection : collection
        }));
    },

    _showTable : function(collection) {

        this.history.show(new Backgrid.Grid({
            columns    : this.columns,
            collection : collection,
            className  : 'table table-hover'
        }));

        this.pager.show(new GridPager({
            columns    : this.columns,
            collection : collection
        }));
    },

    _showToolbar : function() {
        var filterOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'history.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-sonarr-all',
                    callback : this._setFilter
                },
                {
                    key      : 'grabbed',
                    title    : '',
                    tooltip  : 'Grabbed',
                    icon     : 'icon-sonarr-downloading',
                    callback : this._setFilter
                },
                {
                    key      : 'imported',
                    title    : '',
                    tooltip  : 'Imported',
                    icon     : 'icon-sonarr-imported',
                    callback : this._setFilter
                },
                {
                    key      : 'failed',
                    title    : '',
                    tooltip  : 'Failed',
                    icon     : 'icon-sonarr-download-failed',
                    callback : this._setFilter
                },
                {
                    key      : 'deleted',
                    title    : '',
                    tooltip  : 'Deleted',
                    icon     : 'icon-sonarr-deleted',
                    callback : this._setFilter
                }
            ]
        };

        this.toolbar.show(new ToolbarLayout({
            right   : [
                filterOptions
            ],
            context : this
        }));
    },

    _showToolbarMovie : function() {
        var filterOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'history.filterMode',
            defaultAction : 'all',
            items         : [
                {
                    key      : 'all',
                    title    : '',
                    tooltip  : 'All',
                    icon     : 'icon-sonarr-all',
                    callback : this._setFilterMovie
                },
                {
                    key      : 'grabbed',
                    title    : '',
                    tooltip  : 'Grabbed',
                    icon     : 'icon-sonarr-downloading',
                    callback : this._setFilterMovie
                },
                {
                    key      : 'imported',
                    title    : '',
                    tooltip  : 'Imported',
                    icon     : 'icon-sonarr-imported',
                    callback : this._setFilterMovie
                },
                {
                    key      : 'failed',
                    title    : '',
                    tooltip  : 'Failed',
                    icon     : 'icon-sonarr-download-failed',
                    callback : this._setFilterMovie
                },
                {
                    key      : 'deleted',
                    title    : '',
                    tooltip  : 'Deleted',
                    icon     : 'icon-sonarr-deleted',
                    callback : this._setFilterMovie
                }
            ]
        };

        this.toolbarMovie.show(new ToolbarLayout({
            right   : [
                filterOptions
            ],
            context : this
        }));
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.collection.state.currentPage = 1;
        var promise = this.collection.setFilterMode(mode);

        if (buttonContext) {
            buttonContext.ui.icon.spinForPromise(promise);
        }
    },

    _setFilterMovie : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.movieCollection.state.currentPage = 1;
        var promise = this.movieCollection.setFilterMode(mode);

        if (buttonContext) {
            buttonContext.ui.icon.spinForPromise(promise);
        }
    }
});
