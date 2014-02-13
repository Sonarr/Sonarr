'use strict';

define(
    [
        'AppLayout',
        'marionette',
        'Settings/Indexers/DeleteView',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (AppLayout, Marionette, DeleteView, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Indexers/ItemTemplate',
            tagName : 'li',

            events: {
                'click .x-delete': '_deleteIndexer'
            },

            _deleteIndexer: function () {
                var view = new DeleteView({ model: this.model});
                AppLayout.modalRegion.show(view);
            }
        });

        AsModelBoundView.call(view);
        return AsValidatedView.call(view);

    });
