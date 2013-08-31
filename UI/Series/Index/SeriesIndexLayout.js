'use strict';
define(
    [
        'marionette',
        'Series/Index/Posters/CollectionView',
        'Series/Index/List/CollectionView',
        'Series/Index/EmptyView',
        'Series/SeriesCollection',
        'Cells/RelativeDateCell',
        'Cells/SeriesTitleCell',
        'Cells/TemplatedCell',
        'Cells/QualityProfileCell',
        'Cells/EpisodeProgressCell',
        'Shared/Grid/DateHeaderCell',
        'Series/Index/Table/SeriesStatusCell',
        'Series/Index/Table/Row',
        'Series/Index/FooterView',
        'Series/Index/FooterModel',
        'Shared/Toolbar/ToolbarLayout'
    ], function (Marionette,
                 PosterCollectionView,
                 ListCollectionView,
                 EmptyView,
                 SeriesCollection,
                 RelativeDateCell,
                 SeriesTitleCell,
                 TemplatedCell,
                 QualityProfileCell,
                 EpisodeProgressCell,
                 DateHeaderCell,
                 SeriesStatusCell,
                 SeriesIndexRow,
                 FooterView,
                 FooterModel,
                 ToolbarLayout) {
        return Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                seriesRegion: '#x-series',
                toolbar     : '#x-toolbar',
                footer : '#x-series-footer'
            },

            columns:
                [
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
                        headerCell: DateHeaderCell
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
                    this._showFooter();
                }
            },

            onShow: function () {
                this._showToolbar();
                this._renderView();
                this._fetchCollection();
            },

            _fetchCollection: function () {
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
