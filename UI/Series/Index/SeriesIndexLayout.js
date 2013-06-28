'use strict';
define(
    [
        'marionette',
        'Series/Index/Posters/CollectionView',
        'Series/Index/List/CollectionView',
        'Series/Index/EmptyView',
        'Series/SeriesCollection',
        'Cells/AirDateCell',
        'Cells/SeriesTitleCell',
        'Cells/TemplatedCell',
        'Series/Index/Table/SeriesStatusCell',
        'Series/Index/Table/Row',
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView'
    ], function (Marionette, PosterCollectionView, ListCollectionView, EmptyView, SeriesCollection, AirDateCell, SeriesTitleCell, TemplatedCell, SeriesStatusCell, SeriesIndexRow,
        ToolbarLayout, LoadingView) {
        return Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                seriesRegion: '#x-series',
                toolbar     : '#x-toolbar'
            },

            columns:
                [
                    {
                        name : 'status',
                        label: '',
                        cell : SeriesStatusCell
                    },
                    {
                        name : 'this',
                        label: 'Title',
                        cell : SeriesTitleCell
                    },
                    {
                        name : 'seasonCount',
                        label: 'Seasons',
                        cell : 'integer'
                    },
                    {
                        name : 'quality',
                        label: 'Quality',
                        cell : 'integer'
                    },
                    {
                        name : 'network',
                        label: 'Network',
                        cell : 'string'
                    },
                    {
                        name : 'nextAiring',
                        label: 'Next Airing',
                        cell : AirDateCell
                    },
                    {
                        name    : 'this',
                        label   : 'Episodes',
                        sortable: false,
                        template: 'Series/EpisodeProgressTemplate',
                        cell    : TemplatedCell
                    },
                    {
                        name    : 'this',
                        label   : '',
                        sortable: false,
                        template: 'Series/Index/Table/ControlsColumnTemplate',
                        cell    : TemplatedCell
                    }
                ],

            leftSideButtons: {
                type      : 'default',
                storeState: false,
                items     :
                    [
                        {
                            title: 'Add Series',
                            icon : 'icon-plus',
                            route: 'addseries'
                        },
                        {
                            title         : 'RSS Sync',
                            icon          : 'icon-rss',
                            command       : 'rsssync',
                            successMessage: 'RSS Sync Completed',
                            errorMessage  : 'RSS Sync Failed!'
                        },
                        {
                            title         : 'Update Library',
                            icon          : 'icon-refresh',
                            command       : 'refreshseries',
                            successMessage: 'Library was updated!',
                            errorMessage  : 'Library update failed!'
                        }
                    ]
            },

            _showTable: function () {
                this.currentView = new Backgrid.Grid({
                    row       : SeriesIndexRow,
                    collection: SeriesCollection,
                    columns   : this.columns,
                    className : 'table table-hover'
                });

                this._renderView();
                this._fetchCollection();
            },

            _showList: function () {
                this.currentView = new ListCollectionView();
                this._fetchCollection();
            },

            _showPosters: function () {
                this.currentView = new PosterCollectionView();
                this._fetchCollection();
            },


            initialize: function () {
                this.seriesCollection = SeriesCollection;

                this.listenTo(SeriesCollection, 'sync', this._renderView);
                this.listenTo(SeriesCollection, 'remove', this._renderView);
            },


            _renderView: function () {

                if (SeriesCollection.length === 0) {
                    this.seriesRegion.show(new EmptyView());
                    this.toolbar.close();
                }
                else {
                    this.currentView.collection = SeriesCollection;
                    this.seriesRegion.show(this.currentView);

                    this._showToolbar();
                }
            },


            onShow: function () {
                this._showToolbar();
                this._renderView();
                this._fetchCollection();
            },


            _fetchCollection: function () {
                if (SeriesCollection.length === 0) {
                    this.seriesRegion.show(new LoadingView());
                }

                SeriesCollection.fetch();
            },

            _showToolbar: function () {

                if (this.toolbar.currentView) {
                    return;
                }

                var viewButtons = {
                    type         : 'radio',
                    storeState   : true,
                    menuKey      : 'seriesViewMode',
                    defaultAction: 'listView',
                    items        :
                        [
                            {
                                key     : 'tableView',
                                title   : '',
                                icon    : 'icon-table',
                                callback: this._showTable
                            },
                            {
                                key     : 'listView',
                                title   : '',
                                icon    : 'icon-list',
                                callback: this._showList
                            },
                            {
                                key     : 'posterView',
                                title   : '',
                                icon    : 'icon-picture',
                                callback: this._showPosters
                            }
                        ]
                };

                this.toolbar.show(new ToolbarLayout({
                    right  :
                        [
                            viewButtons
                        ],
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            }
        });
    });
