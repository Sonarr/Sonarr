'use strict';
define(
    [
        'vent',
        'Commands/CommandModel',
        'Commands/CommandCollection',
        'Commands/CommandMessengerCollectionView',
        'underscore',
        'jQuery/jquery.spin'
    ], function (vent, CommandModel, CommandCollection, CommandMessengerCollectionView, _) {


        CommandMessengerCollectionView.render();

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

                    CommandCollection.bind('add', function (model) {
                        if (model.isSameCommand(options.command)) {
                            self._bindToCommandModel.call(self, model, options);
                        }
                    });

                    CommandCollection.bind('sync', function () {
                        var command = CommandCollection.findCommand(options.command);
                        if (command) {
                            self._bindToCommandModel.call(self, command, options);
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
                                vent.trigger(vent.Events.CommandComplete, { command: model, model: options.model });
                            }
                        }
                    });

                    options.element.startSpin();
                }
            };
        };

        return singleton();
    });
