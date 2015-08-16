var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var PosterCollectionView = require('./Posters/MoviePostersCollectionView');
var ListCollectionView = require('./Overview/MovieOverviewCollectionView');
var EmptyView = require('./EmptyView');
var MovieCollection = require('../MovieCollection');
var MovieTitleCell = require('../../Cells/MovieTitleCell');
var ProfileCell = require('../../Cells/ProfileCell');
var MoviesActionsCell = require('../../Cells/MovieActionsCell');
var MovieMonitoredCell = require('../../Cells/MovieMonitoredCell');
var MovieDownloadedCell = require('../../Cells/MovieDownloadedCell');
var FooterView = require('./FooterView');
var FooterModel = require('./FooterModel');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Movie/Index/MovieIndexLayoutTemplate',

    regions : {
        moviesRegion : '#x-movies',
        toolbar      : '#x-toolbar',
        toolbar2     : '#x-toolbar2',
        footer       : '#x-movies-footer'
    },

    columns : [
        {
            name  : 'statusWeight',
            label : 'Monitored',
            cell  : MovieMonitoredCell
        },
        {
            name      : 'title',
            label     : 'Title',
            cell      : MovieTitleCell,
            cellValue : 'this',
            sortValue : 'sortTitle'
        },
        {
            name  : 'profileId',
            label : 'Profile',
            cell  : ProfileCell
        },
        {
            name  : 'statusWeight',
            label : 'Files',
            cell  : MovieDownloadedCell
        },
        {
            name     : 'this',
            label    : '',
            sortable : false,
            cell     : MoviesActionsCell
        }
    ],

    leftSideButtons : {
        type       : 'default',
        storeState : false,
        collapse   : true,
        items      : [
            {
                title : 'Add Movies',
                icon  : 'icon-sonarr-add',
                route : 'addmovie'
            },
            {
                title          : 'Update Library',
                icon           : 'icon-sonarr-refresh',
                command        : 'refreshmovie',
                successMessage : 'Library was updated!',
                errorMessage   : 'Library update failed!'
            }
/*            {
                title : 'Season Pass',
                icon  : 'icon-sonarr-monitored',
                route : 'seasonpass'
            },
            {
                title : 'Series Editor',
                icon  : 'icon-sonarr-edit',
                route : 'serieseditor'
            },
            {
                title        : 'RSS Sync',
                icon         : 'icon-sonarr-rss',
                command      : 'rsssync',
                errorMessage : 'RSS Sync Failed!'
            },
            {
                title          : 'Update Library',
                icon           : 'icon-sonarr-refresh',
                command        : 'refreshseries',
                successMessage : 'Library was updated!',
                errorMessage   : 'Library update failed!'
            }*/
        ]
    },

    initialize : function() {
        this.movieCollection = MovieCollection.clone();
        this.movieCollection.shadowCollection.bindSignalR();

        this.listenTo(this.movieCollection.shadowCollection, 'sync', function(model, collection, options) {
            this.movieCollection.fullCollection.resetFiltered();
            this._renderView();
        });

        this.listenTo(this.movieCollection.shadowCollection, 'add', function(model, collection, options) {
            this.movieCollection.fullCollection.resetFiltered();
            this._renderView();
        });

        this.listenTo(this.movieCollection.shadowCollection, 'remove', function(model, collection, options) {
            this.movieCollection.fullCollection.resetFiltered();
            this._renderView();
        });

        this.sortingOptions = {
            type           : 'sorting',
            storeState     : false,
            viewCollection : this.movieCollection,
            items          : [
                {
                    title : 'Title',
                    name  : 'title'
                },
                {
                    title : 'Quality',
                    name  : 'profileId'
                },
				{
                    title : 'Monitored',
                    name  : 'monitored'				
				}
            ]
        };

        this.viewButtons = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'movieViewMode',
            defaultAction : 'listView',
            items         : [
                {
                    key      : 'posterView',
                    title    : '',
                    tooltip  : 'Posters',
                    icon     : 'icon-sonarr-view-poster',
                    callback : this._showPosters
                },
                {
                    key      : 'listView',
                    title    : '',
                    tooltip  : 'Overview List',
                    icon     : 'icon-sonarr-view-list',
                    callback : this._showList
                },
                {
                    key      : 'tableView',
                    title    : '',
                    tooltip  : 'Table',
                    icon     : 'icon-sonarr-view-table',
                    callback : this._showTable
                }
            ]
        };
    },

    onShow : function() {
        this._showToolbar();
        this._fetchCollection();
    },

    _showTable : function() {
        this.currentView = new Backgrid.Grid({
            collection : this.movieCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this._renderView();
    },

    _showList : function() {
        this.currentView = new ListCollectionView({
            collection : this.movieCollection
        });

        this._renderView();
    },

    _showPosters : function() {
        this.currentView = new PosterCollectionView({
            collection : this.movieCollection
        });

        this._renderView();
    },

    _renderView : function() {
        if (MovieCollection.length === 0) {
            this.moviesRegion.show(new EmptyView());

            this.toolbar.close();
            this.toolbar2.close();
        } else {
            this.moviesRegion.show(this.currentView);

            this._showToolbar();
            this._showFooter();
        }
    },

    _fetchCollection : function() {
        this.movieCollection.fetch();
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.movieCollection.setFilterMode(mode);
    },

    _showToolbar : function() {
        if (this.toolbar.currentView) {
            return;
        }

        /*this.toolbar2.show(new ToolbarLayout({
            right   : [
                this.filteringOptions
            ],
            context : this
        }));*/

        this.toolbar.show(new ToolbarLayout({
            right   : [
                this.sortingOptions,
                this.viewButtons
            ],
            left    : [
                this.leftSideButtons
            ],
            context : this
        }));
    },

    _showFooter : function() {
        var footerModel = new FooterModel();
        var movies = MovieCollection.models.length;

        footerModel.set({
            movies       : movies
        });

        this.footer.show(new FooterView({ model : footerModel }));
    }
});