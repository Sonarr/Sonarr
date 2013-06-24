'use strict';
define([
    'app',
    'Series/Index/List/CollectionView',
    'Series/Index/Posters/CollectionView',
    'Series/Index/EmptyView',
    'Series/SeriesCollection',
    'Cells/AirDateCell',
    'Cells/SeriesTitleCell',
    'Cells/SeriesStatusCell',
    'Cells/TemplatedCell',
    'Shared/Toolbar/ToolbarLayout',
    'Config',
    'Shared/LoadingView'
],
    function (
        App,
        ListCollectionView,
        PosterCollectionView,
        EmptyView,
        SeriesCollection,
        AirDateCell,
        SeriesTitleCell,
        SeriesStatusCell,
        TemplatedCell,
        ToolbarLayout,
        Config,
        LoadingView)
    {
        NzbDrone.Series.Index.SeriesIndexLayout = Backbone.Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                series : '#x-series',
                toolbar: '#x-toolbar'
            },

            columns: [
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
                items     : [
                    {
                        title: 'Add Series',
                        icon : 'icon-plus',
                        route: 'series/add'
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
                var view = new Backgrid.Grid(
                    {
                        row       : NzbDrone.Series.Index.Table.Row,
                        columns   : this.columns,
                        collection: this.seriesCollection,
                        className : 'table table-hover'
                    });

                this._fetchCollection(view);
            },

            _showList: function () {
                var view = new ListCollectionView();
                this._fetchCollection(view);
            },

            _showPosters: function () {
                var view = new PosterCollectionView();
                this._fetchCollection(view);
            },

            _showEmpty: function () {
                this.series.show(new EmptyView());
            },

            _fetchCollection: function (view) {
                var self = this;

                if (this.seriesCollection.models.length === 0) {
                    this.series.show(new LoadingView());

                    this.seriesCollection.fetch()
                        .done(function () {
                            if (self.seriesCollection.models.length === 0) {
                                self._showEmpty();
                            }
                            else {
                                view.collection = self.seriesCollection;
                                self.series.show(view);
                            }
                        });
                }

                else {
                    view.collection = this.seriesCollection;
                    this.series.show(view);
                }
            },

            initialize: function () {
                this.seriesCollection = new SeriesCollection();
            },

            onShow: function () {

                //TODO: Move this outside of the function - 'this' is not available for the call back though (use string like events?)
                var viewButtons = {
                    type         : 'radio',
                    storeState   : true,
                    menuKey      : 'seriesViewMode',
                    defaultAction: 'listView',
                    items        : [
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
                    right  : [ viewButtons],
                    left   : [ this.leftSideButtons],
                    context: this
                }));
            }
        });

        return NzbDrone.Series.Index.SeriesIndexLayou;
    });
