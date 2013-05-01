"use strict";
define([
    'app',
    'Missing/Collection',
    'Series/Index/Table/AirDateCell',
    'Series/Index/Table/SeriesStatusCell',
    'Shared/Toolbar/ToolbarView',
    'Shared/Toolbar/ToolbarLayout'
],
    function () {
        NzbDrone.Missing.MissingLayout = Backbone.Marionette.Layout.extend({
            template: 'Missing/MissingLayoutTemplate',

            regions: {
                missing: '#x-missing',
                toolbar: '#x-toolbar'
            },

            showTable: function () {

                var columns = [
                    {
                        name      : 'seriesTitle',
                        label     : 'Series Title',
                        editable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/SeriesTitleTemplate' }),
                        headerCell: 'nzbDrone'
                    },
                    {
                        name      : 'episode',
                        label     : 'Episode',
                        editable  : false,
                        sortable  : false,
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/EpisodeColumnTemplate' }),
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
                        cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/ControlsColumnTemplate' }),
                        headerCell: 'nzbDrone'
                    }
                ];

                this.missing.show(new Backgrid.Grid(
                    {
                        row       : NzbDrone.Missing.Row,
                        columns   : columns,
                        collection: this.missingCollection,
                        className : 'table table-hover'
                    }));
            },

            initialize: function () {
                this.missingCollection = new NzbDrone.Missing.Collection();
                this.missingCollection.fetch();
            },

            onShow: function () {
                this.showTable();
                //this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({right: [ viewButtons], context: this}));
            }

        })
        ;
    })
;
