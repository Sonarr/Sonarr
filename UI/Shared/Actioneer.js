'use strict';
define(
    [
        'Commands/CommandController',
        'Commands/CommandCollection',
        'Shared/Messenger',
        'jQuery/jquery.spin'
    ], function (CommandController, CommandCollection, Messenger) {

        var actioneer = Marionette.AppRouter.extend({

            initialize: function () {
                this.trackedCommands =
                    [
                    ];
                CommandCollection.fetch();
                this.listenTo(CommandCollection, 'sync', this._handleCommands);
            },


            bindToCommand: function (options) {

                var self = this;
                options.idleIcon = this._getIconClass(options.element);

                var existingCommand = CommandCollection.findCommand(options.command);

                if (existingCommand) {
                    this._bindToCommandModel.call(this, existingCommand, options);
                }

                this.listenTo(CommandCollection, 'add sync', function (model) {
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

                this.listenTo(model, 'change:state', function (model) {
                    if (!model.isActive()) {
                        options.element.stopSpin();
                    }
                });

                options.element.startSpin();
            },

            ExecuteCommand: function (options) {
                options.iconClass = this._getIconClass(options.element);

                if (options.button) {
                    options.button.addClass('disable');
                }

                this._setSpinnerOnElement(options);

                var promise = CommandController.Execute(options.command, options.properties);
                this._showStartMessage(options, promise);
            },

            SaveModel: function (options) {
                options.iconClass = this._getIconClass(options.element);

                this._showStartMessage(options);
                this._setSpinnerOnElement(options);

                var model = options.model ? options.model : options.context.model;

                var promise = model.save();
                this._handlePromise(promise, options);
            },

            _handlePromise: function (promise, options) {
                var self = this;

                promise.done(function () {
                    self._onSuccess(options);
                });

                promise.fail(function (ajaxOptions) {
                    if (ajaxOptions.readyState === 0 || ajaxOptions.status === 0) {
                        return;
                    }

                    self._onError(options);
                });

                promise.always(function () {
                    self._onComplete(options);
                });
            },

            _handleCommands: function () {
                var self = this;

                _.each(this.trackedCommands, function (trackedCommand) {
                    if (trackedCommand.completed === true) {
                        return;
                    }

                    var options = trackedCommand.options;
                    var command = CommandCollection.find({ 'id': trackedCommand.id });

                    if (!command) {
                        trackedCommand.completed = true;

                        self._onError(options, trackedCommand.id);
                        self._onComplete(options);
                        return;
                    }

                    if (command.get('state') === 'completed') {
                        trackedCommand.completed = true;

                        self._onSuccess(options, command.get('id'));
                        self._onComplete(options);
                        return;
                    }

                    if (command.get('state') === 'failed') {
                        trackedCommand.completed = true;

                        self._onError(options, command.get('id'));
                        self._onComplete(options);
                    }
                });
            },

            _getIconClass: function (element) {
                if (!element) {
                    return '';
                }

                return element.find('i').attr('class').match(/(?:^|\s)icon\-.+?(?:$|\s)/)[0];
            },

            _setSpinnerOnElement: function (options) {
                if (!options.element) {
                    return;
                }

                if (options.leaveIcon) {
                    options.element.addClass('icon-spin');
                }

                else {
                    options.element.removeClass(options.iconClass);
                    options.element.addClass('icon-nd-spinner');
                }
            },

            _onSuccess: function (options, id) {
                if (options.successMessage) {
                    Messenger.show({
                        id     : id,
                        message: options.successMessage,
                        type   : 'success'
                    });
                }

                if (options.onSuccess) {
                    options.onSuccess.call(options.context);
                }
            },

            _onError: function (options, id) {
                if (options.errorMessage) {
                    Messenger.show({
                        id     : id,
                        message: options.errorMessage,
                        type   : 'error'
                    });
                }

                if (options.onError) {
                    options.onError.call(options.context);
                }
            },

            _onComplete: function (options) {
                if (options.button) {
                    options.button.removeClass('disable');
                }

                if (options.element) {
                    if (options.leaveIcon) {
                        options.element.removeClass('icon-spin');
                    }

                    else {
                        options.element.addClass(options.iconClass);
                        options.element.removeClass('icon-nd-spinner');
                        options.element.removeClass('icon-spin');
                    }
                }

                if (options.always) {
                    options.always.call(options.context);
                }
            },

            _showStartMessage: function (options, promise) {
                var self = this;

                if (!promise) {
                    if (options.startMessage) {
                        Messenger.show({
                            message: options.startMessage
                        });
                    }

                    return;
                }

                promise.done(function (data) {
                    self.trackedCommands.push({ id: data.id, options: options });

                    if (options.startMessage) {
                        Messenger.show({
                            id     : data.id,
                            message: options.startMessage
                        });
                    }
                });
            }
        });

        return new actioneer();
    });
