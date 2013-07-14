'use strict';
define(
    [
        'marionette',
        'backgrid',
        'Logs/LogTimeCell',
        'Logs/LogLevelCell',
        'Shared/Grid/Pager',
        'Logs/Collection'
    ], function (Marionette, Backgrid, LogTimeCell, LogLevelCell, GridPager, LogCollection) {
        return Marionette.Layout.extend({
            template: 'Logs/LayoutTemplate',

            regions: {
                grid   : '#x-grid',
                toolbar: '#x-toolbar',
                pager  : '#x-pager'
            },

            attributes: {
                id: 'logs-screen'
            },

            columns:
                [
                    {
                        name    : 'level',
                        label   : '',
                        sortable: true,
                        cell    : LogLevelCell
                    },
                    {
                        name    : 'logger',
                        label   : 'Component',
                        sortable: true,
                        cell    : Backgrid.StringCell.extend({
                            className: 'log-logger-cell'
                        })
                    },
                    {
                        name    : 'message',
                        label   : 'Message',
                        sortable: false,
                        cell    : Backgrid.StringCell.extend({
                            className: 'log-message-cell'
                        })
                    },
                    {
                        name : 'time',
                        label: 'Time',
                        cell : LogTimeCell
                    }
                ],

            showTable: function () {

                this.grid.show(new Backgrid.Grid({
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

        });
    });
