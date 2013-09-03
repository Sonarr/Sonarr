'use strict';

define([
    'app',
    'marionette',
    'Settings/Notifications/Model',
    'Settings/Notifications/DeleteView',
    'Shared/Messenger',
    'Shared/Actioneer',
    'Mixins/AsModelBoundView',
    'Form/FormBuilder'

], function (App, Marionette, NotificationModel, DeleteView, Messenger, Actioneer, AsModelBoundView) {

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
                    App.vent.trigger(App.Commands.CloseModalCommand);
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
                var properties = {};

                _.each(this.model.get('fields'), function (field) {
                    properties[field.name] = field.value;
                });

                Actioneer.ExecuteCommand({
                    command       : testCommand,
                    properties    : properties,
                    button        : this.ui.testButton,
                    element       : this.ui.testIcon,
                    errorMessage  : 'Failed to test notification settings',
                    successMessage: 'Notification settings tested successfully',
                    always        : this._testOnAlways,
                    context       : this
                });
            }
        },

        _testOnAlways: function () {
            if (!this.isClosed) {
                this.idle = true;
            }
        }
    });

    return AsModelBoundView.call(model);
});
