'use strict';
define(
    [
        'marionette',
        'System/Logs/Table/LogsTableLayout',
        'System/Logs/Files/LogFileLayout'
    ], function (Marionette, LogsTableLayout, LogsFileLayout) {
        return Marionette.Layout.extend({
            template: 'System/Logs/LogsLayoutTemplate',

            ui: {
                tableTab: '.x-table-tab',
                filesTab: '.x-files-tab'
            },

            regions: {
                table: '#table',
                files: '#files'
            },

            events: {
                'click .x-table-tab': '_showTable',
                'click .x-files-tab': '_showFiles'
            },

            onShow: function () {
                this._showTable();
            },

            _showTable: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.tableTab.tab('show');
                this.table.show(new LogsTableLayout());
            },

            _showFiles: function (e) {
                if (e) {
                    e.preventDefault();
                }

                this.ui.filesTab.tab('show');
                this.files.show(new LogsFileLayout());
            }
        });
    });
