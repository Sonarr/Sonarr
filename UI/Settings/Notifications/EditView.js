"use strict";

define([
    'app',
    'Settings/Notifications/Model',
    'Settings/Notifications/DeleteView'

], function () {

    NzbDrone.Settings.Notifications.EditView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Notifications/EditTemplate',

        events: {
            'click .x-save'   : '_saveNotification',
            'click .x-remove' : '_deleteNotification',
            'click .x-test'   : '_test'
        },

        ui: {
            testButton : '.x-test',
            testIcon   : '.x-test-icon'
        },

        initialize: function (options) {
            this.notificationCollection = options.notificationCollection;
        },

        _saveNotification: function () {
            var name = this.model.get('name');
            var success = 'Notification Saved: ' + name;
            var fail = 'Failed to save notification: ' + name;

            this.model.save(undefined, NzbDrone.Settings.SyncNotificaiton.callback({
                successMessage: success,
                errorMessage: fail,
                successCallback: this._saveSuccess,
                context: this
            }));
        },

        _deleteNotification: function () {
            var view = new NzbDrone.Settings.Notifications.DeleteView({ model: this.model });
            NzbDrone.modalRegion.show(view);
        },

        _saveSuccess: function () {
            this.notificationCollection.add(this.model, { merge: true });
            NzbDrone.modalRegion.closeModal();
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
                var commandPromise = NzbDrone.Commands.Execute(testCommand, properties);
                commandPromise.done(function () {
                    NzbDrone.Shared.Messenger.show({
                        message: 'Notification settings tested successfully'
                    });
                });

                commandPromise.fail(function (options) {
                    if (options.readyState === 0 || options.status === 0) {
                        return;
                    }

                    NzbDrone.Shared.Messenger.show({
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
});
