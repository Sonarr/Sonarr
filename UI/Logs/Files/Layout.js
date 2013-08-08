'use strict';
define(
    [
        'app',
        'marionette',
        'backgrid',
        'Logs/Files/FilenameCell',
        'Cells/RelativeDateCell',
        'Logs/Files/Collection',
        'Logs/Files/Row',
        'Logs/Files/ContentsView',
        'Logs/Files/ContentsModel',
        'Shared/Toolbar/ToolbarLayout'
    ], function (App, Marionette, Backgrid, FilenameCell, RelativeDateCell, LogFileCollection, LogFileRow, ContentsView, ContentsModel, ToolbarLayout) {
        return Marionette.Layout.extend({
            template: 'Logs/Files/LayoutTemplate',

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
            },

            onShow: function () {
                this._fetchAndShow();
                this._showToolbar();
                this._showTable();
            },

            _fetchAndShow: function () {
                var self = this;

                var promise = this.collection.fetch();
                promise.done(function () {
                    self._showLogFile({ model: self.collection.first() });
                });
            },

            _showToolbar: function () {

                var leftSideButtons = {
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
                                errorMessage   : 'Failed to delete log files',
                                ownerContext   : this,
                                successCallback: this._refreshLogs
                            }
                        ]
                };

                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            leftSideButtons
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

                this.contents.close();

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
            }
        });
    });
