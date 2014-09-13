'use strict';
define(
    [
        'marionette',
        'backgrid',
        'System/Task/TaskCollection',
        'Cells/RelativeTimeCell',
        'System/Task/TaskIntervalCell',
        'System/Task/ExecuteTaskCell',
        'System/Task/NextExecutionCell',
        'Shared/LoadingView',
        'Mixins/backbone.signalr.mixin'
    ], function (Marionette,
                 Backgrid,
                 BackupCollection,
                 RelativeTimeCell,
                 TaskIntervalCell,
                 ExecuteTaskCell,
                 NextExecutionCell,
                 LoadingView) {
        return Marionette.Layout.extend({
            template: 'System/Task/TaskLayoutTemplate',

            regions: {
                tasks : '#x-tasks'
            },

            columns: [
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

            initialize: function () {
                this.taskCollection = new BackupCollection();

                this.listenTo(this.taskCollection, 'sync', this._showTasks);
                this.taskCollection.bindSignalR();
            },

            onRender: function () {
                this.tasks.show(new LoadingView());

                this.taskCollection.fetch();
            },

            _showTasks: function () {

                this.tasks.show(new Backgrid.Grid({
                    columns   : this.columns,
                    collection: this.taskCollection,
                    className : 'table table-hover'
                }));
            }
        });
    });
