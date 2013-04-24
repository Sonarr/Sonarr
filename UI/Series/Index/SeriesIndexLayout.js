"use strict";
define([
    'app',
    'Series/Index/List/CollectionView',
<<<<<<< HEAD
    'Config'
    'Series/Index/Posters/CollectionView',
    'Series/Index/EmptyView',
    'Config',
    'Series/Index/Table/AirDateCell',
    'Series/Index/Table/SeriesStatusCell'
    'Shared/Toolbar/ToolbarView',
=======
    'Shared/Toolbar/ToolbarLayout',
>>>>>>> added support for multi-button groups to toolbar
    'Config'
],
    function () {
        NzbDrone.Series.Index.SeriesIndexLayout = Backbone.Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                series : '#x-series',
                toolbar: '#x-toolbar'
            },

            ui: {

            },

            events: {
                'click .x-series-change-view': 'changeView'
            },

            showTable: function () {

                var columns =
                    [
                        {
                            name    : 'status',
                            label   : '',
                            editable: false,
                            cell    : 'seriesStatus'
                        },
                        {
                            name    : 'title',
                            label   : 'Title',
                            editable: false,
                            cell    : 'string'
                        },
                        {
                            name    : 'seasonCount',
                            label   : 'Seasons',
                            editable: false,
                            cell    : 'integer'
                        },
                        {
                            name    : 'quality',
                            label   : 'Quality',
                            editable: false,
                            cell    : 'integer'
                        },
                        {
                            name    : 'network',
                            label   : 'Network',
                            editable: false,
                            cell    : 'string'
                        },
                        {
                            name     : 'nextAiring',
                            label    : 'Next Airing',
                            editable : false,
                            cell     : 'datetime',
                            formatter: new Backgrid.AirDateFormatter()
                        },
                        {
                            name    : 'episodes',
                            label   : 'Episodes',
                            editable: false,
                            sortable: false,
                            cell    : 'string'
                        },
                        {
                            name    : 'edit',
                            label   : '',
                            editable: false,
                            sortable: false,
                            cell    : 'string'
                        }
                    ];

                var grid = new Backgrid.Grid(
                    {
                        name: 'status',
                        label: '',
                        editable: false,
                        cell: 'seriesStatus',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name: 'title',
                        label: 'Title',
                        editable: false,
                        cell: Backgrid.TemplateBackedCell.extend({ template: 'Series/Index/Table/SeriesTitleTemplate' }),
                        headerCell: 'nzbDrone'
                    },
                    {
                        name: 'seasonCount',
                        label: 'Seasons',
                        editable: false,
                        cell: 'integer',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name: 'quality',
                        label: 'Quality',
                        editable: false,
                        cell: 'integer',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name: 'network',
                        label: 'Network',
                        editable: false,
                        cell: 'string',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name: 'nextAiring',
                        label: 'Next Airing',
                        editable: false,
                        cell: 'airDate',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name: 'episodes',
                        label: 'Episodes',
                        editable: false,
                        sortable: false,
                        cell: Backgrid.TemplateBackedCell.extend({ template: 'Series/EpisodeProgressTemplate' }),
                        headerCell: 'nzbDrone'
                    },
                    {
                        name: 'edit',
                        label: '',
                        editable: false,
                        sortable: false,
                        cell: Backgrid.TemplateBackedCell.extend({ template: 'Series/Index/Table/ControlsColumnTemplate' }),
                        headerCell: 'nzbDrone'
                    }
                ];

                this.series.show(new Backgrid.Grid(
                    {
                        row: Backgrid.SeriesIndexTableRow,
                        columns : columns,
                        collection : this.seriesCollection,
                        className: 'table table-hover'
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
                this.viewStyle = NzbDrone.Config.SeriesViewStyle();
                this.seriesCollection = new NzbDrone.Series.SeriesCollection();
                this.seriesCollection.fetch();
            },

            onRender: function () {
                var element = this.$('a[data-target="' + this.viewStyle + '"]').removeClass('active');
                this.setActive(element);
            },

            onShow: function () {

                var menuLeft = new NzbDrone.Shared.Toolbar.CommandCollection();

                menuLeft.add(new NzbDrone.Shared.Toolbar.CommandModel({title: "Add Series", icon: "icon-plus"}));
                menuLeft.add(new NzbDrone.Shared.Toolbar.CommandModel({title: "RSS Sync", icon: "icon-rss"}));
                menuLeft.add(new NzbDrone.Shared.Toolbar.CommandModel({title: "Sync Database", icon: "icon-refresh"}));

                var menuRight = new NzbDrone.Shared.Toolbar.CommandCollection();

                menuRight.add(new NzbDrone.Shared.Toolbar.CommandModel({title: "Add Series", icon: "icon-plus"}));
                menuRight.add(new NzbDrone.Shared.Toolbar.CommandModel({title: "RSS Sync", icon: "icon-rss"}));
                menuRight.add(new NzbDrone.Shared.Toolbar.CommandModel({title: "Sync Database", icon: "icon-refresh"}));

                this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({left: [ menuLeft], right: [menuRight]}));

                switch (this.viewStyle) {
                    case 1:
                        this.showList();
                        break;
                    case 2:
                        this.showPosters();
                        break;
                    default:
                        this.showTable();
                }
            },

            changeView: function (e) {
                e.preventDefault();
                var view = parseInt($(e.target).data('target'));
                var target = $(e.target);

                if (isNaN(view)) {
                    view = parseInt($(e.target).parent('a').data('target'));
                    target = $(e.target).parent('a');
                }

                if (view === 1) {
                    NzbDrone.Config.SeriesViewStyle(1);
                    this.showList();
                }

                else if (view === 2) {
                    NzbDrone.Config.SeriesViewStyle(2);
                    this.showPosters();
                }

                else {
                    NzbDrone.Config.SeriesViewStyle(0);
                    this.showTable();
                }

                this.setActive(target);
            },

            setActive: function (element) {
                this.$('a').removeClass('active');
                $(element).addClass('active');
            }
        });
    });
