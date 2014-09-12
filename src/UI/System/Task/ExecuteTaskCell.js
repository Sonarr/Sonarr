'use strict';
define(
    [
        'Cells/NzbDroneCell',
        'Commands/CommandController'
    ], function (NzbDroneCell, CommandController) {
        return NzbDroneCell.extend({

            className: 'execute-task-cell',

            events: {
                'click .x-execute' : '_executeTask'
            },

            render: function () {

                this.$el.empty();

                var name = this.model.get('name');
                var task = this.model.get('taskName');

                this.$el.html(
                    '<i class="icon-refresh icon-can-spin x-execute" title="Execute {0}"></i>'.format(name)
                );

                CommandController.bindToCommand({
                    element: this.$el.find('.x-execute'),
                    command: {
                        name : task
                    }
                });

                return this;
            },

            _executeTask: function () {
                CommandController.Execute(this.model.get('taskName'), {
                    name : this.model.get('taskName')
                });
            }
        });
    });
