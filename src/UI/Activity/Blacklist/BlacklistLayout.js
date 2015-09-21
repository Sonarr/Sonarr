var vent = require('vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var BlacklistCollection = require('./BlacklistCollection');
var SeriesTitleCell = require('../../Cells/SeriesTitleCell');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var QualityCell = require('../../Cells/QualityCell');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var BlacklistActionsCell = require('./BlacklistActionsCell');
var GridPager = require('../../Shared/Grid/Pager');
var LoadingView = require('../../Shared/LoadingView');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');

module.exports = Marionette.Layout.extend({
    template : 'Activity/Blacklist/BlacklistLayoutTemplate',

    regions : {
        blacklist      : '#x-blacklist',
        movieBlacklist : '#x-movie-blacklist',
        toolbar        : '#x-toolbar',
        pager          : '#x-pager',
        moviePager     : '#x-movie-pager'
    },

    columns : [
        {
            name  : 'series',
            label : 'Series',
            cell  : SeriesTitleCell
        },
        {
            name  : 'sourceTitle',
            label : 'Source Title',
            cell  : 'string'
        },
        {
            name     : 'quality',
            label    : 'Quality',
            cell     : QualityCell,
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
            cell     : BlacklistActionsCell,
            sortable : false
        }
    ],

    movieColumns : [
        {
            name  : 'movie',
            label : 'Movie',
            cell  : MovieTitleCell
        },
        {
            name  : 'sourceTitle',
            label : 'Source Title',
            cell  : 'string'
        },
        {
            name     : 'quality',
            label    : 'Quality',
            cell     : QualityCell,
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
            cell     : BlacklistActionsCell,
            sortable : false
        }
    ],

    initialize : function() {
        this.collection = new BlacklistCollection({ tableName : 'blacklist', mediaType : 1 });
        this.movieCollection = new BlacklistCollection({ tableName : 'blacklist', mediaType : 2 });

        this.listenTo(this.collection, 'sync', this._showTable);
        this.listenTo(this.movieCollection, 'sync', this._showMovieTable);
        this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);
    },

    onShow : function() {
        this.blacklist.show(new LoadingView());
        this._showToolbar();
        this.collection.fetch();
        this.movieCollection.fetch();
    },

    _showTable : function(collection) {

        this.blacklist.show(new Backgrid.Grid({
            columns    : this.columns,
            collection : collection,
            className  : 'table table-hover'
        }));

        this.pager.show(new GridPager({
            columns    : this.columns,
            collection : collection
        }));
    },

    _showMovieTable : function(collection) {

        this.movieBlacklist.show(new Backgrid.Grid({
            columns    : this.movieColumns,
            collection : collection,
            className  : 'table table-hover'
        }));

        this.moviePager.show(new GridPager({
            columns    : this.movieColumns,
            collection : collection
        }));
    },

    _showToolbar : function() {
        var leftSideButtons = {
            type       : 'default',
            storeState : false,
            items      : [
                {
                    title   : 'Clear Blacklist',
                    icon    : 'icon-sonarr-clear',
                    command : 'clearBlacklist'
                }
            ]
        };

        this.toolbar.show(new ToolbarLayout({
            left    : [
                leftSideButtons
            ],
            context : this
        }));
    },

    _refreshTable : function(buttonContext) {
        this.collection.state.currentPage = 1;
        var promise = this.collection.fetch({ reset : true });

        if (buttonContext) {
            buttonContext.ui.icon.spinForPromise(promise);
        }
    },

    _commandComplete : function(options) {
        if (options.command.get('name') === 'clearblacklist') {
            this._refreshTable();
        }
    }
});
