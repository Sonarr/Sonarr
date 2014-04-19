'use strict';

define([
    'vent',
    'AppLayout',
    'marionette',
    'Settings/Notifications/Delete/NotificationDeleteView',
    'Commands/CommandController',
    'Mixins/AsModelBoundView',
    'Mixins/AsValidatedView',
    'underscore',
    'Form/FormBuilder'
], function (vent, AppLayout, Marionette, DeleteView, CommandController, AsModelBoundView, AsValidatedView, _) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Notifications/Edit/NotificationEditViewTemplate',

        ui: {
            onDownloadToggle: '.x-on-download',
            onUpgradeSection: '.x-on-upgrade'
       },

        events: {
            'click .x-save'        : '_save',
            'click .x-save-and-add': '_saveAndAdd',
            'click .x-delete'      : '_delete',
            'click .x-back'        : '_back',
            'click .x-cancel'      : '_cancel',
            'click .x-test'        : '_test',
            'change .x-on-download': '_onDownloadChanged'
        },

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        },

        onRender: function () {
            this._onDownloadChanged();
        },

        _save: function () {
            var self = this;
            var promise = this.model.save();

            if (promise) {
                promise.done(function () {
                    self.targetCollection.add(self.model, { merge: true });
                    vent.trigger(vent.Commands.CloseModalCommand);
                });
            }
        },

        _saveAndAdd: function () {
            var self = this;
            var promise = this.model.save();

            if (promise) {
                promise.done(function () {
                    self.targetCollection.add(self.model, { merge: true });

                    require('Settings/Notifications/Add/NotificationSchemaModal').open(self.targetCollection);
                });
            }
        },

        _delete: function () {
            var view = new DeleteView({ model: this.model });
            AppLayout.modalRegion.show(view);
        },

        _back: function () {
            if (this.model.isNew()) {
                this.model.destroy();
            }

            require('Settings/Notifications/Add/NotificationSchemaModal').open(this.targetCollection);
        },

        _cancel: function () {
            if (this.model.isNew()) {
                this.model.destroy();
            }
        },

        _test: function () {
            var testCommand = 'test{0}'.format(this.model.get('implementation'));
            var properties = {};

            _.each(this.model.get('fields'), function (field) {
                properties[field.name] = field.value;
            });

            CommandController.Execute(testCommand, properties);
        },

        _onDownloadChanged: function () {
            var checked = this.ui.onDownloadToggle.prop('checked');

            if (checked) {
                this.ui.onUpgradeSection.show();
            }

            else {
                this.ui.onUpgradeSection.hide();
            }
        }
    });

    AsModelBoundView.call(view);
    AsValidatedView.call(view);

    return view;
});
