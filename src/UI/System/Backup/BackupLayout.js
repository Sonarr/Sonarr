'use strict';
define(
    [
        'vent',
        'marionette',
        'backgrid',
        'System/Backup/BackupCollection',
        'Cells/RelativeDateCell',
        'System/Backup/BackupFilenameCell',
        'System/Backup/BackupTypeCell',
        'System/Backup/BackupEmptyView',
        'Shared/LoadingView',
        'Shared/Toolbar/ToolbarLayout'
    ], function (vent, Marionette, Backgrid, BackupCollection, RelativeDateCell, BackupFilenameCell, BackupTypeCell, EmptyView, LoadingView, ToolbarLayout) {
        return Marionette.Layout.extend({
            template: 'System/Backup/BackupLayoutTemplate',

            regions: {
                backups : '#x-backups',
                toolbar : '#x-backup-toolbar'
            },

            columns: [
                {
                    name     : 'type',
                    label    : '',
                    sortable : false,
                    cell     : BackupTypeCell
                },
                {
                    name     : 'this',
                    label    : 'Name',
                    sortable : false,
                    cell     : BackupFilenameCell
                },
                {
                    name     : 'time',
                    label    : 'Time',
                    sortable : false,
                    cell     : RelativeDateCell
                }
            ],

            leftSideButtons: {
                type      : 'default',
                storeState: false,
                collapse  : false,
                items     :
                    [
                        {
                            title         : 'Backup',
                            icon          : 'icon-file-text',
                            command       : 'backup',
                            properties    : { type: 'manual' },
                            successMessage: 'Database and settings were backed up successfully',
                            errorMessage  : 'Backup Failed!'
                        }
                    ]
            },

            initialize: function () {
                this.backupCollection = new BackupCollection();

                this.listenTo(this.backupCollection, 'sync', this._showUpdates);
                this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);
            },

            onRender: function () {
                this._showToolbar();
                this.backups.show(new LoadingView());

                this.backupCollection.fetch();
            },

            _showUpdates: function () {

                if (this.backupCollection.length === 0) {
                    this.backups.show(new EmptyView());
                }

                else {
                    this.backups.show(new Backgrid.Grid({
                        columns   : this.columns,
                        collection: this.backupCollection,
                        className : 'table table-hover'
                    }));
                }
            },

            _showToolbar : function () {
                this.toolbar.show(new ToolbarLayout({
                    left   :
                        [
                            this.leftSideButtons
                        ],
                    context: this
                }));
            },

            _commandComplete: function (options) {
                if (options.command.get('name') === 'backup') {
                    this.backupCollection.fetch();
                }
            }
        });
    });
