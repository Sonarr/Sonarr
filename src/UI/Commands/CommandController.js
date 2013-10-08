'use strict';
define(
    [
        'app',
        'Commands/CommandModel',
        'Commands/CommandCollection',
        'underscore',
        'jQuery/jquery.spin'
    ], function (App, CommandModel, CommandCollection, _) {

        var singleton = function () {

            return {

                Execute: function (name, properties) {

                    var attr = _.extend({name: name.toLocaleLowerCase()}, properties);

                    var commandModel = new CommandModel(attr);

                    return commandModel.save().success(function () {
                        CommandCollection.add(commandModel);
                    });
                },

                bindToCommand: function (options) {

                    var self = this;

                    var existingCommand = CommandCollection.findCommand(options.command);

                    if (existingCommand) {
                        this._bindToCommandModel.call(this, existingCommand, options);
                    }

                    CommandCollection.bind('add sync', function (model) {
                        if (model.isSameCommand(options.command)) {
                            self._bindToCommandModel.call(self, model, options);
                        }
                    });
                },

                _bindToCommandModel: function bindToCommand(model, options) {

                    if (!model.isActive()) {
                        options.element.stopSpin();
                        return;
                    }

                    model.bind('change:state', function (model) {
                        if (!model.isActive()) {
                            options.element.stopSpin();

                            if (model.isComplete()) {
                                App.vent.trigger(App.Events.CommandComplete, { command: model, model: options.model });
                            }
                        }
                    });

                    options.element.startSpin();
                }
            };
        };

        return singleton();
    });
