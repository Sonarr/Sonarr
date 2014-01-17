'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Series/Index/Posters/CollectionView',
        'Series/Index/List/CollectionView',
        'Series/Index/EmptyView',
        'Series/SeriesCollection',
        'Cells/RelativeDateCell',
        'Cells/SeriesTitleCell',
        'Cells/TemplatedCell',
        'Cells/QualityProfileCell',
        'Cells/EpisodeProgressCell',
        'Cells/SeriesActionsCell',
        'Cells/SeriesStatusCell',
        'Series/Index/FooterView',
        'Series/Index/FooterModel',
        'Shared/Toolbar/ToolbarLayout',
        'underscore'
    ], function (Marionette,
                 Backgrid,
                 PosterCollectionView,
                 ListCollectionView,
                 EmptyView,
                 SeriesCollection,
                 RelativeDateCell,
                 SeriesTitleCell,
                 TemplatedCell,
                 QualityProfileCell,
                 EpisodeProgressCell,
                 SeriesActionsCell,
                 SeriesStatusCell,
                 FooterView,
                 FooterModel,
                 ToolbarLayout,
                 _) {
        return Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                seriesRegion  : '#x-series',
                toolbar       : '#x-toolbar',
                footer        : '#x-series-footer'
            },

            columns: [
                {
                    name      : 'statusWeight',
                    label     : '',
                    cell      : SeriesStatusCell
                },
                {
                    name     : 'title',
                    label    : 'Title',
                    cell     : SeriesTitleCell,
                    cellValue: 'this'
                },
                {
                    name : 'seasonCount',
                    label: 'Seasons',
                    cell : 'integer'
                },
                {
                    name : 'qualityProfileId',
                    label: 'Quality',
                    cell : QualityProfileCell
                },
                {
                    name : 'network',
                    label: 'Network',
                    cell : 'string'
                },
                {
                    name      : 'nextAiring',
                    label     : 'Next Airing',
                    cell      : RelativeDateCell,
                    sortValue : SeriesCollection.nextAiring
                },
                {
                    name     : 'percentOfEpisodes',
                    label    : 'Episodes',
                    cell     : EpisodeProgressCell,
                    className: 'episode-progress-cell'
                },
                {
                    name    : 'this',
                    label   : '',
                    sortable: false,
                    cell    : SeriesActionsCell
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
                            title  : 'Season Pass',
                            icon   : 'icon-bookmark',
                            route  : 'seasonpass'
                        },
                        {
                            title  : 'Series Editor',
                            icon   : 'icon-nd-edit',
                            route  : 'serieseditor'
                        },
                        {
                            title         : 'RSS Sync',
                            icon          : 'icon-rss',
                            command       : 'rsssync',
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

            sortingOptions: {
                type          : 'sorting',
                storeState    : false,
                viewCollection: SeriesCollection,
                items         :
                    [
                        {
                            title: 'Title',
                            name : 'title'
                        },
                        {
                            title: 'Seasons',
                            name : 'seasonCount'
                        },
                        {
                            title: 'Quality',
                            name : 'qualityProfileId'
                        },
                        {
                            title: 'Network',
                            name : 'network'
                        },
                        {
                            title     : 'Next Airing',
                            name      : 'nextAiring',
                            sortValue : SeriesCollection.nextAiring
                        },
                        {
                            title: 'Episodes',
                            name : 'percentOfEpisodes'
                        }
                    ]
            },

            initialize: function () {
                this.seriesCollection = SeriesCollection;

                this.listenTo(SeriesCollection, 'sync', this._renderView);
                this.listenTo(SeriesCollection, 'remove', this._renderView);

                this.viewButtons = {
                    type         : 'radio',
                    storeState   : true,
                    menuKey      : 'seriesViewMode',
                    defaultAction: 'listView',
                    items        :
                        [
                            {
                                key     : 'posterView',
                                title   : '',
                                tooltip : 'Posters',
                                icon    : 'icon-th-large',
                                callback: this._showPosters
                            },
                            {
                                key     : 'listView',
                                title   : '',
                                tooltip : 'Overview List',
                                icon    : 'icon-th-list',
                                callback: this._showList
                            },
                            {
                                key     : 'tableView',
                                title   : '',
                                tooltip : 'Table',
                                icon    : 'icon-table',
                                callback: this._showTable
                            }
                        ]
                };
            },

            _showTable: function () {
                this.currentView = new Backgrid.Grid({
                    collection: SeriesCollection,
                    columns   : this.columns,
                    className : 'table table-hover'
                });

                this._fetchCollection();
            },

            _showList: function () {
                this.currentView = new ListCollectionView({ collection: SeriesCollection });

                this._fetchCollection();
            },

            _showPosters: function () {
                this.currentView = new PosterCollectionView({ collection: SeriesCollection });

                this._fetchCollection();
            },

            _renderView: function () {

                if (SeriesCollection.length === 0) {
                    this.seriesRegion.show(new EmptyView());
                    this.toolbar.close();
                }
                else {
                    this.seriesRegion.show(this.currentView);

                    this._showToolbar();
                    this._showFooter();
                }
            },

            onShow: function () {
                this._showToolbar();
                this._renderView();
            },

            _fetchCollection: function () {
                SeriesCollection.fetch();
            },

            _showToolbar: function () {

                if (this.toolbar.currentView) {
                    return;
                }

                var rightButtons = [
                    this.viewButtons
                ];

                rightButtons.splice(0, 0, this.sortingOptions);

                this.toolbar.show(new ToolbarLayout({
                    right  : rightButtons,
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            },

            _showFooter: function () {
                var footerModel = new FooterModel();
                var series = SeriesCollection.models.length;
                var episodes = 0;
                var episodeFiles = 0;
                var ended = 0;
                var continuing = 0;
                var monitored = 0;

                _.each(SeriesCollection.models, function (model){
                    episodes += model.get('episodeCount');
                    episodeFiles += model.get('episodeFileCount');

                    if (model.get('status').toLowerCase() === 'ended') {
                        ended++;
                    }

                    else {
                        continuing++;
                    }

                    if (model.get('monitored')) {
                        monitored++;
                    }
                });

                footerModel.set({
                    series: series,
                    ended: ended,
                    continuing: continuing,
                    monitored: monitored,
                    unmonitored: series - monitored,
                    episodes: episodes,
                    episodeFiles: episodeFiles
                });

                this.footer.show(new FooterView({ model: footerModel }));
            }
        });
    });
