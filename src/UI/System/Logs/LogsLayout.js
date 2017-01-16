var Marionette = require('marionette');
var LogsTableLayout = require('./Table/LogsTableLayout');
var LogsFileLayout = require('./Files/LogFileLayout');
var LogFileCollection = require('./Files/LogFileCollection');
var UpdateLogFileCollection = require('./Updates/LogFileCollection');

module.exports = Marionette.Layout.extend({
    template : 'System/Logs/LogsLayoutTemplate',

    ui : {
        tableTab       : '.x-table-tab',
        filesTab       : '.x-files-tab',
        updateFilesTab : '.x-update-files-tab'
    },

    regions : {
        table       : '#table',
        files       : '#files',
        updateFiles : '#update-files'
    },

    events : {
        'click .x-table-tab'        : '_showTable',
        'click .x-files-tab'        : '_showFiles',
        'click .x-update-files-tab' : '_showUpdateFiles'
    },

    onShow : function() {
        this._showTable();
    },

    _showTable : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.tableTab.tab('show');
        this.table.show(new LogsTableLayout());
    },

    _showFiles : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.filesTab.tab('show');
        this.files.show(new LogsFileLayout({
            collection         : new LogFileCollection(),
            deleteFilesCommand : 'deleteLogFiles'
        }));
    },

    _showUpdateFiles : function(e) {
        if (e) {
            e.preventDefault();
        }

        this.ui.updateFilesTab.tab('show');
        this.updateFiles.show(new LogsFileLayout({
            collection         : new UpdateLogFileCollection(),
            deleteFilesCommand : 'deleteUpdateLogFiles'
        }));
    }
});