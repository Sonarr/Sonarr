"use strict";
define([
    'app',
    'Series/Index/List/CollectionView',
    'Series/Index/Posters/CollectionView',
    'Series/Index/EmptyView',
    'Series/Index/Table/AirDateCell',
    'Series/Index/Table/SeriesStatusCell',
    'Shared/Toolbar/ToolbarLayout',
    'Config'
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

            showTable: function () {

                this.series.show(new Backgrid.Grid(
                    {
                        row       : NzbDrone.Series.Index.Table.Row,
                        columns   : this.columns,
                        collection: this.seriesCollection,
                        className : 'table table-hover'
                    }));
            },

            showList: function () {
                this.series.show(new NzbDrone.Series.Index.List.CollectionView({ collection: this.seriesCollection }));
            },

            showPosters: function () {
                this.series.show(new NzbDrone.Series.Index.Posters.CollectionView({ collection: this.seriesCollection }));
            },

            showEmpty: function () {
                this.series.show(new NzbDrone.Series.Index.EmptyView());
            },

            initialize: function () {
                this.seriesCollection = new NzbDrone.Series.SeriesCollection();
                this.seriesCollection.fetch();
            },

            onShow: function () {

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
                            callback: this.showTable
                        },
                        {
                            key     : 'listView',
                            title   : '',
                            icon    : 'icon-list',
                            callback: this.showList
                        },
                        {
                            key     : 'posterView',
                            title   : '',
                            icon    : 'icon-picture',
                            callback: this.showPosters
                        }
                    ]
                };


                var leftSideButtons = {
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
                        },
                        {
                            title         : 'Test Action',
                            icon          : 'icon-asterisk',
                            command       : 'test',
                            successMessage: 'Test Completed',
                            errorMessage  : 'Test Failed!'
                        }
                    ]
                };

                this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({
                    right  : [ viewButtons],
                    left   : [ leftSideButtons],
                    context: this
                }));
            }

        })
        ;
    })
;
