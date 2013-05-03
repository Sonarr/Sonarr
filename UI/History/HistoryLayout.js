"use strict";
define([
    'app',
    'History/Collection',
    'Series/Index/Table/AirDateCell',
    'Shared/Toolbar/ToolbarLayout'
],
    function () {
        NzbDrone.History.HistoryLayout = Backbone.Marionette.Layout.extend({
            template: 'History/HistoryLayoutTemplate',

            regions: {
                history: '#x-history',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
            },

            showTable: function () {

                var columns = [
                    {
                        name      : 'seriesTitle',
                        label     : 'Series Title',
                        editable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'History/SeriesTitleTemplate' }),
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'episode',
                        label     : 'Episode',
                        editable  : false,
                        sortable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'History/EpisodeColumnTemplate' }),
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'title',
                        label     : 'Episode Title',
                        editable  : false,
                        sortable  : false,
                        cell      : 'string',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'airDate',
                        label     : 'Air Date',
                        editable  : false,
                        cell      : 'airDate',
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'edit',
                        label     : '',
                        editable  : false,
                        sortable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'History/ControlsColumnTemplate' }),
                        headerCell: 'nzbDrone'
                    }
                ];

                this.history.show(new Backgrid.Grid(
                    {
                        row       : NzbDrone.History.Row,
                        columns   : columns,
                        collection: this.historyCollection,
                        className : 'table table-hover'
                    }));

                this.pager.show(new Backgrid.NzbDronePaginator({
                    columns: columns,
                    collection: this.historyCollection
                }));
            },

            initialize: function () {
                this.historyCollection = new NzbDrone.History.Collection();
                this.historyCollection.fetch();
            },

            onShow: function () {
                this.showTable();
                //this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({right: [ viewButtons], context: this}));
            }

        })
        ;
    })
;
