'use strict';

define([
    'AppLayout',
    'marionette',
    'Settings/DownloadClient/Edit/DownloadClientEditView'
], function (AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template: 'Settings/DownloadClient/DownloadClientItemViewTemplate',
        tagName : 'li',

        events: {
            'click'  : '_edit'
        },

        initialize: function () {
            this.listenTo(this.model, 'sync', this.render);
        },

        _edit: function () {
            var view = new EditView({ model: this.model, targetCollection: this.model.collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
