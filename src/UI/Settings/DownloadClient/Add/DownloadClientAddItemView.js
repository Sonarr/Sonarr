'use strict';

define([
    'AppLayout',
    'marionette',
    'Settings/DownloadClient/Edit/DownloadClientEditView'
], function (AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template: 'Settings/DownloadClient/Add/DownloadClientAddItemViewTemplate',
        tagName : 'li',

        events: {
            'click': '_add'
        },

        initialize: function (options) {
            this.downloadClientCollection = options.downloadClientCollection;
        },

        _add: function (e) {
            if (this.$(e.target).hasClass('icon-info-sign')) {
                return;
            }

            this.model.set({
                id         : undefined,
                name       : this.model.get('implementationName'),
                onGrab     : true,
                onDownload : true,
                onUpgrade  : true
            });

            var editView = new EditView({ model: this.model, downloadClientCollection: this.downloadClientCollection });
            AppLayout.modalRegion.show(editView);
        }
    });
});
