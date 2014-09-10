'use strict';

define([
    'underscore',
    'vent',
    'AppLayout',
    'marionette',
    'Settings/Notifications/Delete/NotificationDeleteView',
    'Commands/CommandController',
    'Mixins/AsModelBoundView',
    'Mixins/AsValidatedView',
    'Mixins/AsEditModalView',
    'Form/FormBuilder',
    'Mixins/TagInput'
], function (_, vent, AppLayout, Marionette, DeleteView, CommandController, AsModelBoundView, AsValidatedView, AsEditModalView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Notifications/Edit/NotificationEditViewTemplate',

        ui: {
            onDownloadToggle : '.x-on-download',
            onUpgradeSection : '.x-on-upgrade',
            tags             : '.x-tags'
        },

        events: {
            'click .x-back'        : '_back',
            'change .x-on-download': '_onDownloadChanged'
        },

        _deleteView: DeleteView,

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        },

        onRender: function () {
            this._onDownloadChanged();

            this.ui.tags.tagInput({
                model    : this.model,
                property : 'tags'
            });
        },

        _onAfterSave: function () {
            this.targetCollection.add(this.model, { merge: true });
            vent.trigger(vent.Commands.CloseModalCommand);
        },

        _onAfterSaveAndAdd: function () {
            this.targetCollection.add(this.model, { merge: true });

            require('Settings/Notifications/Add/NotificationSchemaModal').open(this.targetCollection);
        },

        _back: function () {
            if (this.model.isNew()) {
                this.model.destroy();
            }

            require('Settings/Notifications/Add/NotificationSchemaModal').open(this.targetCollection);
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
    AsEditModalView.call(view);

    return view;
});
