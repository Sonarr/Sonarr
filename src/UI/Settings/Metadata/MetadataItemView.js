'use strict';

define(
    [
        'AppLayout',
        'marionette',
        'Settings/Metadata/MetadataEditView',
        'Mixins/AsModelBoundView'
    ], function (AppLayout, Marionette, EditView, AsModelBoundView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Metadata/MetadataItemViewTemplate',
            tagName : 'li',

            events: {
                'click .x-edit'  : '_edit'
            },

            initialize: function () {
                this.listenTo(this.model, 'sync', this.render);
            },

            _edit: function () {
                var view = new EditView({ model: this.model});
                AppLayout.modalRegion.show(view);
            }
        });

        return AsModelBoundView.call(view);
    });
