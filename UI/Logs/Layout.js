'use strict';
define([
    'marionette',
    'backgrid',
    'Shared/Grid/Pager',
    'Logs/Collection'
],
    function (Marionette,Backgrid, GridPager, LogCollection) {
        return Marionette.Layout.extend({
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

                this.pager.show(new GridPager({
                    columns   : this.columns,
                    collection: this.collection
                }));
            },

            initialize: function () {
                this.collection = new LogCollection();
                this.collection.fetch();
            },

            onShow: function () {
                this.showTable();
            }

        })
        ;
    })
;
