"use strict";
define([
    'app',
    'Series/Index/List/CollectionView',
    'Series/Index/Posters/CollectionView',
    'Series/Index/EmptyView',
    'Series/Index/Table/AirDateCell',
    'Series/Index/Table/SeriesStatusCell',
    'Shared/Toolbar/ToolbarView',
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

            showTable: function () {

                var columns = [
                    {
                        name      : 'status',
                        label     : '',
                        editable  : false,
                        cell      : 'seriesStatus',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'title',
                        label     : 'Title',
                        editable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'Series/Index/Table/SeriesTitleTemplate' }),
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'seasonCount',
                        label     : 'Seasons',
                        editable  : false,
                        cell      : 'integer',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'quality',
                        label     : 'Quality',
                        editable  : false,
                        cell      : 'integer',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'network',
                        label     : 'Network',
                        editable  : false,
                        cell      : 'string',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'nextAiring',
                        label     : 'Next Airing',
                        editable  : false,
                        cell      : 'airDate',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'episodes',
                        label     : 'Episodes',
                        editable  : false,
                        sortable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'Series/EpisodeProgressTemplate' }),
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'edit',
                        label     : '',
                        editable  : false,
                        sortable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'Series/Index/Table/ControlsColumnTemplate' }),
                        headerCell: 'nzbDrone'
                    }
                ];

                this.series.show(new Backgrid.Grid(
                    {
                        row       : NzbDrone.Series.Index.Table.Row,
                        columns   : columns,
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
                    storeState   : 'true',
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

                this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({right: [ viewButtons], context: this}));
            }

        })
        ;
    })
;
