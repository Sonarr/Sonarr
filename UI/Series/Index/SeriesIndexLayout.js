"use strict";
define([
    'app',
    'Series/Index/List/CollectionView',
    'Config'
],
    function (app) {
        NzbDrone.Series.Index.SeriesIndexLayout = Backbone.Marionette.Layout.extend({
            template: 'Series/Index/SeriesIndexLayoutTemplate',

            regions: {
                series: '#x-series'
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
                        name: 'status',
                        label: '',
                        editable: false,
                        cell: 'seriesStatus'
                    },
                    {
                        name: 'title',
                        label: 'Title',
                        editable: false,
                        cell: 'string'
                    },
                    {
                        name: 'seasonCount',
                        label: 'Seasons',
                        editable: false,
                        cell: 'integer'
                    },
                    {
                        name: 'quality',
                        label: 'Quality',
                        editable: false,
                        cell: 'integer'
                    },
                    {
                        name: 'network',
                        label: 'Network',
                        editable: false,
                        cell: 'string'
                    },
                    {
                        name: 'nextAiring',
                        label: 'Next Airing',
                        editable: false,
                        cell: 'airDate'
                    },
                    {
                        name: 'episodes',
                        label: 'Episodes',
                        editable: false,
                        sortable: false,
                        cell: 'episodeProgress'
                    },
                    {
                        name: 'edit',
                        label: '',
                        editable: false,
                        sortable: false,
                        cell: 'controlsColumn'
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
                switch (this.viewStyle) {
                    case 1:
                        this.showList();
                        break;
                    default:
                        this.showTable();
                }
            },

            changeView: function (e) {
                e.preventDefault();
                var view = parseInt($(e.target).data('target'));

                if (isNaN(view)) {
                    view = parseInt($(e.target).parent('a').data('target'));
                    this.setActive($(e.target).parent('a'));
                }

                else{
                    this.setActive($(e.target));
                }

                if (view === 1) {
                    NzbDrone.Config.SeriesViewStyle(1);
                    this.showList();
                }

                else {
                    NzbDrone.Config.SeriesViewStyle(0);
                    this.showTable();
                }
            },

            setActive: function (element) {
                this.$('a').removeClass('active');
                $(element).addClass('active');
            }
        });
    });
