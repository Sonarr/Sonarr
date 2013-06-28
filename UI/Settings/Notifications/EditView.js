'use strict';

define([
    'app',
    'marionette',
    'Settings/Notifications/Model',
    'Settings/Notifications/DeleteView',
    'Shared/Messenger',
    'Mixins/AsModelBoundView',
    'Form/FormBuilder'

], function (App, Marionette, NotificationModel, DeleteView, Messenger, AsModelBoundView) {

    var model = Marionette.ItemView.extend({
        template: 'Settings/Notifications/EditTemplate',

        events: {
            'click .x-save'          : '_saveNotification',
            'click .x-save-and-add'  : '_saveAndAddNotification',
            'click .x-delete'        : '_deleteNotification',
            'click .x-back'          : '_back',
            'click .x-test'          : '_test'
        },

        ui: {
            testButton: '.x-test',
            testIcon  : '.x-test-icon'
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        },

        _saveNotification: function () {
            var self = this;
            var promise = this.model.saveSettings();

            if (promise) {
                promise.done(function () {
                    self.notificationCollection.add(self.model, { merge: true });
                    App.modalRegion.closeModal();
                });
            }
        },

        _saveAndAddNotification: function () {
            var self = this;
            var promise = this.model.saveSettings();

            if (promise) {
                promise.done(function () {
                    self.notificationCollection.add(self.model, { merge: true });

                    require('Settings/Notifications/SchemaModal').open(self.notificationCollection);
                });
            }
        },

        _deleteNotification: function () {
            var view = new DeleteView({ model: this.model });
            App.modalRegion.show(view);
        },

        _back: function () {
            require('Settings/Notifications/SchemaModal').open(this.notificationCollection);
        },

        _test: function () {
            var testCommand = this.model.get('testCommand');
            if (testCommand) {
                this.idle = false;
                this.ui.testButton.addClass('disabled');
                this.ui.testIcon.addClass('icon-spinner icon-spin');

                var properties = {};

                _.each(this.model.get('fields'), function (field) {
                    properties[field.name] = field.value;
                });

                var self = this;
                var commandPromise = App.Commands.Execute(testCommand, properties);
                commandPromise.done(function () {
                    Messenger.show({
                        message: 'Notification settings tested successfully'
                    });
                });

                commandPromise.fail(function (options) {
                    if (options.readyState === 0 || options.status === 0) {
                        return;
                    }

                    Messenger.show({
                        message: 'Failed to test notification settings',
                        type   : 'error'
                    });
                });

                commandPromise.always(function () {
                    if (!self.isClosed) {
                        self.ui.testButton.removeClass('disabled');
                        self.ui.testIcon.removeClass('icon-spinner icon-spin');
                        self.idle = true;
                    }
                });
            }
        }
    });

    return AsModelBoundView.call(model);
});
