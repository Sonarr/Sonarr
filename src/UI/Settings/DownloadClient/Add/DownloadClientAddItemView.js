'use strict';

define([
    'jquery',
    'AppLayout',
    'marionette',
    'Settings/DownloadClient/Edit/DownloadClientEditView'
], function ($, AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template: 'Settings/DownloadClient/Add/DownloadClientAddItemViewTemplate',
        tagName : 'li',

        events: {
            'click': '_add'
        },

        initialize: function (options) {
            this.targetCollection = options.targetCollection;
        },

        _add: function (e) {
            if (this.$(e.target).hasClass('icon-info-sign')) {
                return;
            }

            this.model.set({
                id         : undefined,
                name       : this.model.get('implementation'),
                enable     : true
            });

            var editView = new EditView({ model: this.model, targetCollection: this.targetCollection });
            AppLayout.modalRegion.show(editView);
        }
    });
});
