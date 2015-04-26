var vent = require('vent');
var CommandModel = require('./CommandModel');
var CommandCollection = require('./CommandCollection');
var CommandMessengerCollectionView = require('./CommandMessengerCollectionView');
var _ = require('underscore');
var moment = require('moment');
var Messenger = require('../Shared/Messenger');
require('../jQuery/jquery.spin');

CommandMessengerCollectionView.render();

var singleton = function() {

    return {

        _lastCommand : {},

        Execute : function(name, properties) {

            var attr = _.extend({ name : name.toLocaleLowerCase() }, properties);
            var commandModel = new CommandModel(attr);

            if (this._lastCommand.command && this._lastCommand.command.isSameCommand(attr) && moment().add('seconds', -5).isBefore(this._lastCommand.time)) {

                Messenger.show({
                    message   : 'Please wait at least 5 seconds before running this command again',
                    hideAfter : 5,
                    type      : 'error'
                });

                return this._lastCommand.promise;
            }

            var promise = commandModel.save().success(function() {
                CommandCollection.add(commandModel);
            });

            this._lastCommand = {
                command : commandModel,
                promise : promise,
                time    : moment()
            };

            return promise;
        },

        bindToCommand : function(options) {

            var self = this;
            var existingCommand = CommandCollection.findCommand(options.command);

            if (existingCommand) {
                this._bindToCommandModel.call(this, existingCommand, options);
            }

            CommandCollection.bind('add', function(model) {
                if (model.isSameCommand(options.command)) {
                    self._bindToCommandModel.call(self, model, options);
                }
            });

            CommandCollection.bind('sync', function() {
                var command = CommandCollection.findCommand(options.command);
                if (command) {
                    self._bindToCommandModel.call(self, command, options);
                }
            });
        },

        _bindToCommandModel : function bindToCommand (model, options) {

            if (!model.isActive()) {
                options.element.stopSpin();
                return;
            }

            model.bind('change:status', function(model) {
                if (!model.isActive()) {
                    options.element.stopSpin();

                    if (model.isComplete()) {
                        vent.trigger(vent.Events.CommandComplete, {
                            command : model,
                            model   : options.model
                        });
                    }
                }
            });

            options.element.startSpin();
        }
    };
};
module.exports = singleton();
