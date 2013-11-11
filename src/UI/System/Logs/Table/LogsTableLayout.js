'use strict';
define(
    [
        'vent',
        'marionette',
        'backgrid',
        'System/Logs/Table/LogTimeCell',
        'System/Logs/Table/LogLevelCell',
        'System/Logs/Table/LogRow',
        'Shared/Grid/Pager',
        'System/Logs/LogsCollection',
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView',
        'jQuery/jquery.spin'
    ], function (vent, Marionette, Backgrid, LogTimeCell, LogLevelCell, LogRow, GridPager, LogCollection, ToolbarLayout, LoadingView) {
        return Marionette.Layout.extend({
            template: 'System/Logs/Table/LogsTableLayoutTemplate',

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

            initialize: function () {
                this.collection = new LogCollection();

                this.listenTo(this.collection, 'sync', this._showTable);
                vent.on(vent.Events.CommandComplete, this._commandComplete, this);
            },

            onRender: function () {
                this.grid.show(new LoadingView());
                this.collection.fetch();
            },

            onShow: function () {
                this._showToolbar();
            },

            _showTable: function () {
                this.grid.show(new Backgrid.Grid({
                    row       : LogRow,
                    columns   : this.columns,
                    collection: this.collection,
                    className : 'table table-hover'
                }));

                this.pager.show(new GridPager({
                    columns   : this.columns,
                    collection: this.collection
                }));
            },

            _showToolbar: function () {
                var rightSideButtons = {
                    type      : 'default',
                        storeState: false,
                        items     :
                    [
                        {
                            title         : 'Refresh',
                            icon          : 'icon-refresh',
                            ownerContext  : this,
                            callback      : this._refreshLogs
                        },

                        {
                            title          : 'Clear Logs',
                            icon           : 'icon-trash',
                            command        : 'clearLog'
                        }
                    ]
                };

                this.toolbar.show(new ToolbarLayout({
                    right   :
                        [
                            rightSideButtons
                        ],
                    context: this
                }));
            },

            _refreshLogs: function (buttonContext) {
                this.collection.state.currentPage = 1;
                var promise = this.collection.fetch({ reset: true });
                buttonContext.ui.icon.spinForPromise(promise);
            },

            _commandComplete: function (options) {
                if (options.command.get('name') === 'clearlog') {
                    this._refreshLogs();
                }
            }
        });
    });
