'use strict';

define(
    [
        'app',
        'marionette',
        'Settings/Notifications/DeleteView',
        'Mixins/AsModelBoundView',
        'Mixins/AsValidatedView'
    ], function (App, Marionette, DeleteView, AsModelBoundView, AsValidatedView) {

        var view = Marionette.ItemView.extend({
            template: 'Settings/Indexers/ItemTemplate',
            tagName : 'li',

            events: {
                'click .x-delete': '_deleteIndexer'
            },

            _deleteIndexer: function () {
                var view = new DeleteView({ model: this.model});
                App.modalRegion.show(view);
            }
        });

        AsModelBoundView.call(view);
        return AsValidatedView.call(view);

    });
