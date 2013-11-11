'use strict';
define(
    [
        'vent',
        'marionette',
        'backgrid',
        'System/Logs/Files/FilenameCell',
        'Cells/RelativeDateCell',
        'System/Logs/Files/DownloadLogCell',
        'System/Logs/Files/LogFileCollection',
        'System/Logs/Files/Row',
        'System/Logs/Files/ContentsView',
        'System/Logs/Files/ContentsModel',
        'Shared/Toolbar/ToolbarLayout',
        'Shared/LoadingView',
        'jQuery/jquery.spin'
    ], function (vent,
        Marionette,
        Backgrid,
        FilenameCell,
        RelativeDateCell,
        DownloadLogCell,
        LogFileCollection,
        LogFileRow,
        ContentsView,
        ContentsModel,
        ToolbarLayout,
        LoadingView) {
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
                    },
                    {
                        name    : 'filename',
                        label   : '',
                        cell    : DownloadLogCell,
                        sortable: false
                    }
                ],

            initialize: function () {
                this.collection = new LogFileCollection();

                vent.on(vent.Commands.ShowLogFile, this._fetchLogFileContents, this);
                vent.on(vent.Events.CommandComplete, this._commandComplete, this);
                this.listenTo(this.collection, 'sync', this._collectionSynced);

                this.collection.fetch();
            },

            onShow: function () {
                this._showToolbar();
                this._showTable();
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

            _collectionSynced: function () {
                if (!this.collection.any()) {
                    return;
                }

                var model = this.collection.first();
                this._fetchLogFileContents({ model: model });
            },

            _fetchLogFileContents: function (options) {
                this.contents.show(new LoadingView());

                var model = options.model;
                var filename = model.get('filename');

                var contentsModel = new ContentsModel({
                    filename: filename
                });

                this.listenToOnce(contentsModel, 'sync', this._showDetails);

                contentsModel.fetch({ dataType: 'text' });
            },

            _showDetails: function (model) {
                this.contents.show(new ContentsView({ model: model }));
            },

            _refreshLogs: function (buttonContext) {
                this.contents.close();
                var promise = this.collection.fetch();
                buttonContext.ui.icon.spinForPromise(promise);
            },

            _commandComplete: function (options) {
                if (options.command.get('name') === 'deletelogfiles') {
                    this._refreshLogs();
                }
            }
        });
    });
