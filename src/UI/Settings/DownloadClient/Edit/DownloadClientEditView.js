'use strict';

define([
    'underscore',
    'vent',
    'AppLayout',
    'marionette',
    'Settings/DownloadClient/Delete/DownloadClientDeleteView',
    'Commands/CommandController',
    'Mixins/AsModelBoundView',
    'Mixins/AsValidatedView',
    'Mixins/AsEditModalView',
    'Form/FormBuilder',
    'Mixins/FileBrowser',
    'bootstrap'
], function (_, vent, AppLayout, Marionette, DeleteView, CommandController, AsModelBoundView, AsValidatedView, AsEditModalView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/DownloadClient/Edit/DownloadClientEditViewTemplate',

        ui: {
            path      : '.x-path',
            modalBody : '.modal-body'
        },

        events: {
            'click .x-back' : '_back'
        },

        _deleteView: DeleteView,

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        },

        onShow: function () {
            //Hack to deal with modals not overflowing
            if (this.ui.path.length > 0) {
                this.ui.modalBody.addClass('modal-overflow');
            }

            this.ui.path.fileBrowser();
        },

        _onAfterSave: function () {
            this.targetCollection.add(this.model, { merge: true });
            vent.trigger(vent.Commands.CloseModalCommand);
        },

        _onAfterSaveAndAdd: function () {
            this.targetCollection.add(this.model, { merge: true });

            require('Settings/DownloadClient/Add/DownloadClientSchemaModal').open(this.targetCollection);
        },

        _back: function () {
            require('Settings/DownloadClient/Add/DownloadClientSchemaModal').open(this.targetCollection);
        }
    });

    AsModelBoundView.call(view);
    AsValidatedView.call(view);
    AsEditModalView.call(view);

    return view;
});
