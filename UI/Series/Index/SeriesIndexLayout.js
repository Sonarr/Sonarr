"use strict";
define([
    'app',
    'Series/Index/List/CollectionView',
    'Series/Index/Posters/CollectionView',
    'Series/Index/EmptyView',
    'Series/Index/Table/AirDateCell',
    'Series/Index/Table/SeriesStatusCell',
    'Shared/Toolbar/ToolbarLayout',
    'Config',
    'Shared/LoadingView'
],
    function () {
        NzbDrone.Series.Index.SeriesIndexLayout = Backbone.Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                series : '#x-series',
                toolbar: '#x-toolbar'
            },

            columns: [
                {
                    name      : 'status',
                    label     : '',
                    cell      : 'seriesStatus'
                },
                {
                    name      : 'title',
                    label     : 'Title',
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Series/Index/Table/SeriesTitleTemplate' })
                },
                {
                    name      : 'seasonCount',
                    label     : 'Seasons',
                    cell      : 'integer'
                },
                {
                    name      : 'quality',
                    label     : 'Quality',
                    cell      : 'integer'
                },
                {
                    name      : 'network',
                    label     : 'Network',
                    cell      : 'string'
                },
                {
                    name      : 'nextAiring',
                    label     : 'Next Airing',
                    cell      : 'airDate'
                },
                {
                    name      : 'episodes',
                    label     : 'Episodes',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Series/EpisodeProgressTemplate' })
                },
                {
                    name      : 'edit',
                    label     : '',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Series/Index/Table/ControlsColumnTemplate' })
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
                var view = new NzbDrone.Series.Index.List.CollectionView();
                this._fetchCollection(view);
            },

            _showPosters: function () {
                var view = new NzbDrone.Series.Index.Posters.CollectionView();
                this._fetchCollection(view);
            },

            _showEmpty: function () {
                this.series.show(new NzbDrone.Series.Index.EmptyView());
            },

            _fetchCollection: function (view) {
                var self = this;

                if (this.seriesCollection.models.length === 0) {
                    this.series.show(new NzbDrone.Shared.LoadingView());

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
                this.seriesCollection = new NzbDrone.Series.SeriesCollection();
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

                this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({
                    right  : [ viewButtons],
                    left   : [ this.leftSideButtons],
                    context: this
                }));
            }
        });
    });
