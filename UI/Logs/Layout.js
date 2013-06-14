"use strict";
define([
    'app',
    'Logs/Collection',
    'Shared/Toolbar/ToolbarLayout',
    'Shared/Grid/Pager'
],
    function () {
        NzbDrone.Logs.Layout = Backbone.Marionette.Layout.extend({
            template: 'Logs/LayoutTemplate',

            regions: {
                grid   : '#x-grid',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
            },

            columns: [
                {
                    name    : 'level',
                    label   : 'Level',
                    sortable: true,
                    cell    : Backgrid.StringCell
                },
                {
                    name    : 'logger',
                    label   : 'Component',
                    sortable: true,
                    cell    : Backgrid.StringCell
                },
                {
                    name    : 'message',
                    label   : 'Message',
                    sortable: false,
                    cell    : Backgrid.StringCell
                },
                {
                    name : 'time',
                    label: 'Time',
                    cell : Backgrid.DatetimeCell
                }
            ],

            showTable: function () {

                this.grid.show(new Backgrid.Grid(
                    {
                        row       : Backgrid.Row,
                        columns   : this.columns,
                        collection: this.collection,
                        className : 'table table-hover'
                    }));

                this.pager.show(new NzbDrone.Shared.Grid.Pager({
                    columns   : this.columns,
                    collection: this.collection
                }));
            },

            initialize: function () {
                this.collection = new NzbDrone.Logs.Collection();
                this.collection.fetch();
            },

            onShow: function () {
                this.showTable();
                //this.toolbar.show(new NzbDrone.Shared.Toolbar.ToolbarLayout({right: [ viewButtons], context: this}));
            }

        })
        ;
    })
;
