var _ = require('underscore');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var PosterCollectionView = require('./Posters/SeriesPostersCollectionView');
var ListCollectionView = require('./Overview/SeriesOverviewCollectionView');
var EmptyView = require('./EmptyView');
var SeriesCollection = require('../SeriesCollection');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var SeriesTitleCell = require('../../Cells/SeriesTitleCell');
var TemplatedCell = require('../../Cells/TemplatedCell');
var ProfileCell = require('../../Cells/ProfileCell');
var EpisodeProgressCell = require('../../Cells/EpisodeProgressCell');
var SeriesActionsCell = require('../../Cells/SeriesActionsCell');
var SeriesStatusCell = require('../../Cells/SeriesStatusCell');
var FooterView = require('./FooterView');
var FooterModel = require('./FooterModel');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'Series/Index/SeriesIndexLayoutTemplate',

    regions : {
        seriesRegion : '#x-series',
        toolbar      : '#x-toolbar',
        toolbar2     : '#x-toolbar2',
        footer       : '#x-series-footer'
    },

    columns : [
        {
            name  : 'statusWeight',
            label : '',
            cell  : SeriesStatusCell
        },
        {
            name      : 'title',
            label     : 'Title',
            cell      : SeriesTitleCell,
            cellValue : 'this',
            sortValue : 'sortTitle'
        },
        {
            name  : 'seasonCount',
            label : 'Seasons',
            cell  : 'integer'
        },
        {
            name  : 'profileId',
            label : 'Profile',
            cell  : ProfileCell
        },
        {
            name  : 'network',
            label : 'Network',
            cell  : 'string'
        },
        {
            name  : 'nextAiring',
            label : 'Next Airing',
            cell  : RelativeDateCell
        },
        {
            name      : 'percentOfEpisodes',
            label     : 'Episodes',
            cell      : EpisodeProgressCell,
            className : 'episode-progress-cell'
        },
        {
            name     : 'this',
            label    : '',
            sortable : false,
            cell     : SeriesActionsCell
        }
    ],

    leftSideButtons : {
        type       : 'default',
        storeState : false,
        collapse   : true,
        items      : [
            {
                title : 'Add Series',
                icon  : 'icon-sonarr-add',
                route : 'addseries'
            },
            {
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
            }
        ]
    },

    initialize : function() {
        this.seriesCollection = SeriesCollection.clone();
        this.seriesCollection.shadowCollection.bindSignalR();

        this.listenTo(this.seriesCollection.shadowCollection, 'sync', function(model, collection, options) {
            this.seriesCollection.fullCollection.resetFiltered();
            this._renderView();
        });

        this.listenTo(this.seriesCollection.shadowCollection, 'add', function(model, collection, options) {
            this.seriesCollection.fullCollection.resetFiltered();
            this._renderView();
        });

        this.listenTo(this.seriesCollection.shadowCollection, 'remove', function(model, collection, options) {
            this.seriesCollection.fullCollection.resetFiltered();
            this._renderView();
        });

        this.sortingOptions = {
            type           : 'sorting',
            storeState     : false,
            viewCollection : this.seriesCollection,
            items          : [
                {
                    title : 'Title',
                    name  : 'title'
                },
                {
                    title : 'Seasons',
                    name  : 'seasonCount'
                },
                {
                    title : 'Quality',
                    name  : 'profileId'
                },
                {
                    title : 'Network',
                    name  : 'network'
                },
                {
                    title : 'Next Airing',
                    name  : 'nextAiring'
                },
                {
                    title : 'Episodes',
                    name  : 'percentOfEpisodes'
                }
            ]
        };

        this.filteringOptions = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'series.filterMode',
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
                    key      : 'monitored',
                    title    : '',
                    tooltip  : 'Monitored Only',
                    icon     : 'icon-sonarr-monitored',
                    callback : this._setFilter
                },
                {
                    key      : 'continuing',
                    title    : '',
                    tooltip  : 'Continuing Only',
                    icon     : 'icon-sonarr-series-continuing',
                    callback : this._setFilter
                },
                {
                    key      : 'ended',
                    title    : '',
                    tooltip  : 'Ended Only',
                    icon     : 'icon-sonarr-series-ended',
                    callback : this._setFilter
                },
                {
                    key      : 'missing',
                    title    : '',
                    tooltip  : 'Missing',
                    icon     : 'icon-sonarr-missing',
                    callback : this._setFilter
                }
            ]
        };

        this.viewButtons = {
            type          : 'radio',
            storeState    : true,
            menuKey       : 'seriesViewMode',
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
            collection : this.seriesCollection,
            columns    : this.columns,
            className  : 'table table-hover'
        });

        this._renderView();
    },

    _showList : function() {
        this.currentView = new ListCollectionView({
            collection : this.seriesCollection
        });

        this._renderView();
    },

    _showPosters : function() {
        this.currentView = new PosterCollectionView({
            collection : this.seriesCollection
        });

        this._renderView();
    },

    _renderView : function() {
        if (SeriesCollection.length === 0) {
            this.seriesRegion.show(new EmptyView());

            this.toolbar.close();
            this.toolbar2.close();
        } else {
            this.seriesRegion.show(this.currentView);

            this._showToolbar();
            this._showFooter();
        }
    },

    _fetchCollection : function() {
        this.seriesCollection.fetch();
    },

    _setFilter : function(buttonContext) {
        var mode = buttonContext.model.get('key');

        this.seriesCollection.setFilterMode(mode);
    },

    _showToolbar : function() {
        if (this.toolbar.currentView) {
            return;
        }

        this.toolbar2.show(new ToolbarLayout({
            right   : [
                this.filteringOptions
            ],
            context : this
        }));

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
        var series = SeriesCollection.models.length;
        var episodes = 0;
        var episodeFiles = 0;
        var ended = 0;
        var continuing = 0;
        var monitored = 0;

        _.each(SeriesCollection.models, function(model) {
            episodes += model.get('episodeCount');
            episodeFiles += model.get('episodeFileCount');

            if (model.get('status').toLowerCase() === 'ended') {
                ended++;
            } else {
                continuing++;
            }

            if (model.get('monitored')) {
                monitored++;
            }
        });

        footerModel.set({
            series       : series,
            ended        : ended,
            continuing   : continuing,
            monitored    : monitored,
            unmonitored  : series - monitored,
            episodes     : episodes,
            episodeFiles : episodeFiles
        });

        this.footer.show(new FooterView({ model : footerModel }));
    }
});
