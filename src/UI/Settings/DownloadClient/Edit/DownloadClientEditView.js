'use strict';

define([
    'vent',
    'AppLayout',
    'marionette',
    'Settings/DownloadClient/Delete/DownloadClientDeleteView',
    'Commands/CommandController',
    'Mixins/AsModelBoundView',
    'Mixins/AsValidatedView',
    'underscore',
    'Form/FormBuilder',
    'Mixins/AutoComplete',
    'bootstrap'
], function (vent, AppLayout, Marionette, DeleteView, CommandController, AsModelBoundView, AsValidatedView, _) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/DownloadClient/Edit/DownloadClientEditViewTemplate',

        ui: {
            path      : '.x-path',
            modalBody : '.modal-body'
        },

        events: {
            'click .x-save'        : '_save',
            'click .x-save-and-add': '_saveAndAdd',
            'click .x-delete'      : '_delete',
            'click .x-back'        : '_back',
            'click .x-test'        : '_test'
        },

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        },

        onShow: function () {
            //Hack to deal with modals not overflowing
            if (this.ui.path.length > 0) {
                this.ui.modalBody.addClass('modal-overflow');
            }

            this.ui.path.autoComplete('/directories');
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

                    require('Settings/DownloadClient/Add/DownloadClientSchemaModal').open(self.targetCollection);
                });
            }
        },

        _delete: function () {
            var view = new DeleteView({ model: this.model });
            AppLayout.modalRegion.show(view);
        },

        _back: function () {
            require('Settings/DownloadClient/Add/DownloadClientSchemaModal').open(this.targetCollection);
        },

        _test: function () {
            this.model.test();
        }
    });

    AsModelBoundView.call(view);
    AsValidatedView.call(view);

    return view;
});
