'use strict';

define([
    'AppLayout',
    'marionette',
    'Settings/Indexers/Restriction/RestrictionEditView'
], function (AppLayout, Marionette, EditView) {

    return Marionette.ItemView.extend({
        template  : 'Settings/Indexers/Restriction/RestrictionItemViewTemplate',
        className : 'row',

        ui: {
            tags: '.x-tags'
        },

        events: {
            'click .x-edit' : '_edit'
        },

        initialize: function () {
            this.listenTo(this.model, 'sync', this.render);
        },

        _edit: function() {
            var view = new EditView({ model: this.model, targetCollection: this.model.collection});
            AppLayout.modalRegion.show(view);
        }
    });
});
