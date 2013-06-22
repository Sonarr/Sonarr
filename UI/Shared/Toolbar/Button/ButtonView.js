'use strict';
define(['app', 'Config', 'Commands/CommandController', 'Shared/Messenger'], function () {

    NzbDrone.Shared.Toolbar.ButtonView = Backbone.Marionette.ItemView.extend({
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
        },

        onClick: function () {
            if (this.idle) {
                this.invokeCallback();
                this.invokeRoute();
                this.invokeCommand();
            }
        },


        invokeCommand: function () {
            var command = this.model.get('command');
            if (command) {
                this.idle = false;
                this.$el.addClass('disabled');
                this.ui.icon.addClass('icon-spinner icon-spin');

                var self = this;
                var commandPromise = NzbDrone.Commands.Execute(command);
                commandPromise.done(function () {
                    if (self.model.get('successMessage')) {
                        NzbDrone.Shared.Messenger.show({
                            message: self.model.get('successMessage')
                        });
                    }
                });

                commandPromise.fail(function (options) {
                    if (options.readyState === 0 || options.status === 0) {
                        return;
                    }
                    if (self.model.get('errorMessage')) {
                        NzbDrone.Shared.Messenger.show({
                            message: self.model.get('errorMessage'),
                            type   : 'error'
                        });
                    }
                });

                commandPromise.always(function () {
                    if (!self.isClosed) {
                        self.$el.removeClass('disabled');
                        self.ui.icon.removeClass('icon-spinner icon-spin');
                        self.idle = true;
                    }
                });
            }
        },

        invokeRoute: function () {
            var route = this.model.get('route');
            if (route) {
                NzbDrone.Router.navigate(route, {trigger: true});
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




