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

                var task = this.model.get('name');

                this.$el.html(
                    '<i class="icon-cogs x-execute" title="Execute {0}"></i>'.format(task)
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
                CommandController.Execute(this.model.get('name'), {
                    name : this.model.get('name')
                });
            }
        });
    });
