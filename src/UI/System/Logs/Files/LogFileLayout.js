'use strict';
define(
    [
        'app',
        'marionette',
        'backgrid',
        'System/Logs/Files/FilenameCell',
        'Cells/RelativeDateCell',
        'System/Logs/Files/LogFileCollection',
        'System/Logs/Files/Row',
        'System/Logs/Files/ContentsView',
        'System/Logs/Files/ContentsModel',
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView'
    ], function (App, Marionette, Backgrid, FilenameCell, RelativeDateCell, LogFileCollection, LogFileRow, ContentsView, ContentsModel, ToolbarLayout, LoadingView) {
        return Marionette.Layout.extend({
            template: 'System/Logs/Files/LogFileLayoutTemplate',

            regions: {
                toolbar  : '#x-toolbar',
                grid     : '#x-grid',
                contents : '#x-contents'
            },

            columns:
                [
                    {
                        name : 'filename',
                        label: 'Filename',
                        cell : FilenameCell
                    },
                    {
                        name : 'lastWriteTime',
                        label: 'Last Write Time',
                        cell : RelativeDateCell
                    }
                ],

            initialize: function () {
                this.collection = new LogFileCollection();

                App.vent.on(App.Commands.ShowLogFile, this._showLogFile, this);
                App.vent.on(App.Events.CommandComplete, this._commandComplete, this);
            },

            onShow: function () {
                this._fetchAndShow();
                this._showToolbar();
                this._showTable();
            },

            _fetchAndShow: function () {
                var self = this;

                this.contents.close();

                var promise = this.collection.fetch();
                promise.done(function () {
                    if (self.collection.length > 0) {
                        self._showLogFile({ model: self.collection.first() });
                    }
                });
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
                                title          : 'Delete Log Files',
                                icon           : 'icon-trash',
                                command        : 'deleteLogFiles',
                                successMessage : 'Log files have been deleted',
                                errorMessage   : 'Failed to delete log files'
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

            _showTable: function () {
                this.grid.show(new Backgrid.Grid({
                    row       : LogFileRow,
                    columns   : this.columns,
                    collection: this.collection,
                    className : 'table table-hover'
                }));
            },

            _showLogFile: function (options) {
                this.contents.show(new LoadingView());

                if (!options.model) {
                    return;
                }

                var self = this;
                var filename = options.model.get('filename');

                $.ajax({
                    url: '/log/' + filename,
                    success: function (data) {
                        var model = new ContentsModel({
                            filename: filename,
                            contents: data
                        });

                        self.contents.show(new ContentsView({ model: model }));
                    }
                });
            },

            _refreshLogs: function () {
                this._fetchAndShow();
            },

            _commandComplete: function (options) {
                if (options.command.get('name') === 'deletelogfiles') {
                    this._refreshLogs();
                }
            }
        });
    });
