'use strict';

define([
    'AppLayout',
    'marionette',
    'Settings/Indexers/Edit/IndexerEditView'
], function (AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template: 'Settings/Indexers/IndexerItemViewTemplate',
        tagName : 'li',

        events: {
            'click'   : '_edit'
        },

        initialize: function () {
            this.listenTo(this.model, 'sync', this.render);
        },

        _edit: function () {
            var view = new EditView({ model: this.model, targetCollection: this.model.collection });
            AppLayout.modalRegion.show(view);
        }
    });
});
