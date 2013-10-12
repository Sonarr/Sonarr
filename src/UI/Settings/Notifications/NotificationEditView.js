'use strict';

define(
    [
        'vent',
        'AppLayout',
        'marionette',
        'Settings/Notifications/DeleteView',
        'Commands/CommandController',
        'Mixins/AsModelBoundView',
        'underscore',
        'Form/FormBuilder'

    ], function (vent, AppLayout, Marionette, DeleteView, CommandController, AsModelBoundView, _) {

        var model = Marionette.ItemView.extend({
            template: 'Settings/Notifications/NotificationEditViewTemplate',

            events: {
                'click .x-save'        : '_saveNotification',
                'click .x-save-and-add': '_saveAndAddNotification',
                'click .x-delete'      : '_deleteNotification',
                'click .x-back'        : '_back',
                'click .x-test'        : '_test'
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
                        vent.trigger(vent.Commands.CloseModalCommand);
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
                AppLayout.modalRegion.show(view);
            },

            _back: function () {
                require('Settings/Notifications/SchemaModal').open(this.notificationCollection);
            },

            _test: function () {
                var testCommand = 'test{0}'.format(this.model.get('implementation'));
                var properties = {};

                _.each(this.model.get('fields'), function (field) {
                    properties[field.name] = field.value;
                });

                CommandController.Execute(testCommand, properties);
            }
        });

        return AsModelBoundView.call(model);
    });
