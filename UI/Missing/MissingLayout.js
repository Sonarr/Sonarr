"use strict";
define([
    'app',
    'Missing/Collection',
    'Series/Index/Table/AirDateCell',
    'Series/Index/Table/SeriesStatusCell',
    'Shared/Toolbar/ToolbarLayout'
],
    function () {
        NzbDrone.Missing.MissingLayout = Backbone.Marionette.Layout.extend({
            template: 'Missing/MissingLayoutTemplate',

            regions: {
                missing: '#x-missing',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
            },

            columns: [
                {
                    name      : 'series.Title',
                    label     : 'Series Title',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/SeriesTitleTemplate' })
                },
                {
                    name      : 'episode',
                    label     : 'Episode',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/EpisodeColumnTemplate' })
                },
                {
                    name      : 'title',
                    label     : 'Episode Title',
                    sortable  : false,
                    cell      : 'string'
                },
                {
                    name      : 'airDate',
                    label     : 'Air Date',
                    cell      : 'airDate'
                },
                {
                    name      : 'edit',
                    label     : '',
                    sortable  : false,
                    cell      : Backgrid.TemplateBackedCell.extend({ template: 'Missing/ControlsColumnTemplate' })
                }
            ],

            showTable: function () {
                this.missing.show(new Backgrid.Grid(
                    {
                        row       : NzbDrone.Missing.Row,
                        columns   : this.columns,
                        collection: this.missingCollection,
                        className : 'table table-hover'
                    }));

                this.pager.show(new Backgrid.NzbDronePaginator({
                    columns: this.columns,
                    collection: this.missingCollection
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
