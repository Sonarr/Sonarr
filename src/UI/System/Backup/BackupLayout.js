var vent = require('../../vent');
var Marionette = require('marionette');
var Backgrid = require('backgrid');
var BackupCollection = require('./BackupCollection');
var RelativeDateCell = require('../../Cells/RelativeDateCell');
var BackupFilenameCell = require('./BackupFilenameCell');
var BackupTypeCell = require('./BackupTypeCell');
var EmptyView = require('./BackupEmptyView');
var LoadingView = require('../../Shared/LoadingView');
var ToolbarLayout = require('../../Shared/Toolbar/ToolbarLayout');

module.exports = Marionette.Layout.extend({
    template         : 'System/Backup/BackupLayoutTemplate',
    regions          : {
        backups : '#x-backups',
        toolbar : '#x-backup-toolbar'
    },
    columns          : [{
        name     : 'type',
        label    : '',
        sortable : false,
        cell     : BackupTypeCell
    }, {
        name     : 'this',
        label    : 'Name',
        sortable : false,
        cell     : BackupFilenameCell
    }, {
        name     : 'time',
        label    : 'Time',
        sortable : false,
        cell     : RelativeDateCell
    }],
    leftSideButtons  : {
        type       : 'default',
        storeState : false,
        collapse   : false,
        items      : [{
            title          : 'Backup',
            icon           : 'icon-file-text',
            command        : 'backup',
            properties     : {type : 'manual'},
            successMessage : 'Database and settings were backed up successfully',
            errorMessage   : 'Backup Failed!'
        }]
    },
    initialize       : function(){
        this.backupCollection = new BackupCollection();
        this.listenTo(this.backupCollection, 'sync', this._showBackups);
        this.listenTo(vent, vent.Events.CommandComplete, this._commandComplete);
    },
    onRender         : function(){
        this._showToolbar();
        this.backups.show(new LoadingView());
        this.backupCollection.fetch();
    },
    _showBackups     : function(){
        if(this.backupCollection.length === 0) {
            this.backups.show(new EmptyView());
        }
        else {
            this.backups.show(new Backgrid.Grid({
                columns    : this.columns,
                collection : this.backupCollection,
                className  : 'table table-hover'
            }));
        }
    },
    _showToolbar     : function(){
        this.toolbar.show(new ToolbarLayout({
            left    : [this.leftSideButtons],
            context : this
        }));
    },
    _commandComplete : function(options){
        if(options.command.get('name') === 'backup') {
            this.backupCollection.fetch();
        }
    }
});