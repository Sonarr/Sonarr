'use strict';
define(
    [
        'app',
        'marionette',
        'Commands/CommandController',
        'Shared/Messenger'
    ], function (App, Marionette, CommandController, Messenger) {

        return Marionette.ItemView.extend({
            template : 'Shared/Toolbar/ButtonTemplate',
            className: 'btn',

            events: {
                'click': 'onClick'
            },

            ui: {
                icon: '.x-icon'
            },


            initialize: function () {
                this.storageKey = this.model.get('menuKey') + ':' + this.model.get('key');
                this.idle = true;
            },

            onRender: function () {
                if (this.model.get('active')) {
                    this.$el.addClass('active');
                    this.invokeCallback();
                }

                if(!this.model.get('title')){
                    this.$el.addClass('btn-icon-only');
                }
            },

            onClick: function () {
                if (this.idle) {
                    this.invokeCallback();
                    this.invokeRoute();
                    this.invokeCommand();
                }
            },

            invokeCommand: function () {
                //TODO: Use Actioneer to handle icon swapping

                var command = this.model.get('command');
                if (command) {
                    this.idle = false;
                    this.$el.addClass('disabled');
                    this.ui.icon.addClass('icon-spinner icon-spin');

                    var self = this;
                    var commandPromise = CommandController.Execute(command);
                    commandPromise.done(function () {
                        if (self.model.get('successMessage')) {
                            Messenger.show({
                                message: self.model.get('successMessage')
                            });
                        }

                        if (self.model.get('successCallback')) {
                            if (!self.model.ownerContext) {
                                throw 'ownerContext must be set.';
                            }

                            self.model.get('successCallback').call(self.model.ownerContext);
                        }
                    });

                    commandPromise.fail(function (options) {
                        if (options.readyState === 0 || options.status === 0) {
                            return;
                        }

                        if (self.model.get('errorMessage')) {
                            Messenger.show({
                                message: self.model.get('errorMessage'),
                                type   : 'error'
                            });
                        }

                        if (self.model.get('failCallback')) {
                            if (!self.model.ownerContext) {
                                throw 'ownerContext must be set.';
                            }

                            self.model.get('failCallback').call(self.model.ownerContext);
                        }
                    });

                    commandPromise.always(function () {
                        if (!self.isClosed) {
                            self.$el.removeClass('disabled');
                            self.ui.icon.removeClass('icon-spinner icon-spin');
                            self.idle = true;
                        }
                    });

                    if (self.model.get('alwaysCallback')) {
                        if (!self.model.ownerContext) {
                            throw 'ownerContext must be set.';
                        }

                        self.model.get('alwaysCallback').call(self.model.ownerContext);
                    }
                }
            },

            invokeRoute: function () {
                var route = this.model.get('route');
                if (route) {

                    require(
                        [
                            'Router'
                        ], function () {
                            App.Router.navigate(route, {trigger: true});
                        });
                }
            },

            invokeCallback: function () {

                if (!this.model.ownerContext) {
                    throw 'ownerContext must be set.';
                }

                var callback = this.model.get('callback');
                if (callback) {
                    callback.call(this.model.ownerContext);
                }
            }

        });
    });




