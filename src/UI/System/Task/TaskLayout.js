var Marionette = require('marionette');
var Backgrid = require('backgrid');
var BackupCollection = require('./TaskCollection');
var RelativeTimeCell = require('../../Cells/RelativeTimeCell');
var TaskIntervalCell = require('./TaskIntervalCell');
var ExecuteTaskCell = require('./ExecuteTaskCell');
var NextExecutionCell = require('./NextExecutionCell');
var LoadingView = require('../../Shared/LoadingView');
require('../../Mixins/backbone.signalr.mixin');

module.exports = Marionette.Layout.extend({
    template : 'System/Task/TaskLayoutTemplate',

    regions : {
        tasks : '#x-tasks'
    },

    columns : [
        {
            name     : 'name',
            label    : 'Name',
            sortable : true,
            cell     : 'string'
        },
        {
            name     : 'interval',
            label    : 'Interval',
            sortable : true,
            cell     : TaskIntervalCell
        },
        {
            name     : 'lastExecution',
            label    : 'Last Execution',
            sortable : true,
            cell     : RelativeTimeCell
        },
        {
            name     : 'nextExecution',
            label    : 'Next Execution',
            sortable : true,
            cell     : NextExecutionCell
        },
        {
            name     : 'this',
            label    : '',
            sortable : false,
            cell     : ExecuteTaskCell
        }
    ],

    initialize : function() {
        this.taskCollection = new BackupCollection();

        this.listenTo(this.taskCollection, 'sync', this._showTasks);
        this.taskCollection.bindSignalR();
    },

    onRender : function() {
        this.tasks.show(new LoadingView());

        this.taskCollection.fetch();
    },

    _showTasks : function() {
        this.tasks.show(new Backgrid.Grid({
            columns    : this.columns,
            collection : this.taskCollection,
            className  : 'table table-hover'
        }));
    }
});