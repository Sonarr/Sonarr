'use strict';

define(
    [
        'AppLayout',
        'marionette',
        'Settings/DownloadClient/Edit/DownloadClientEditView',
        'Settings/DownloadClient/Delete/DownloadClientDeleteView'
    ], function (AppLayout, Marionette, EditView, DeleteView) {

        return Marionette.ItemView.extend({
            template: 'Settings/DownloadClient/DownloadClientItemViewTemplate',
            tagName : 'li',

            events: {
                'click .x-edit'   : '_edit',
                'click .x-delete' : '_delete'
            },

            initialize: function () {
                this.listenTo(this.model, 'sync', this.render);
            },

            _edit: function () {
                var view = new EditView({ model: this.model, downloadClientCollection: this.model.collection });
                AppLayout.modalRegion.show(view);
            },

            _delete: function () {
                var view = new DeleteView({ model: this.model});
                AppLayout.modalRegion.show(view);
            }
        });
    });
