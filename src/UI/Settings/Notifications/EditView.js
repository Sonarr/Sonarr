'use strict';

define(
    [
        'vent',
        'AppLayout',
        'marionette',
        'Settings/Notifications/DeleteView',
        'Commands/CommandController',
        'Mixins/AsModelBoundView',
        'Form/FormBuilder',
        'underscore'

    ], function (vent, AppLayout, Marionette, DeleteView, CommandController, AsModelBoundView, _) {

        var model = Marionette.ItemView.extend({
            template: 'Settings/Notifications/EditTemplate',

            events: {
                'click .x-save'        : '_saveNotification',
                'click .x-save-and-add': '_saveAndAddNotification',
                'click .x-delete'      : '_deleteNotification',
                'click .x-back'        : '_back',
                'click .x-test'        : '_test'
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
                var testCommand = this.model.get('testCommand');
                if (testCommand) {
                    this.idle = false;
                    var properties = {};

                    _.each(this.model.get('fields'), function (field) {
                        properties[field.name] = field.value;
                    });


                    CommandController.Execute(testCommand, properties);
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
