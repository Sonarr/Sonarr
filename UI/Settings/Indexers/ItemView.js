'use strict';

define([
    'app',
    'marionette',
    'Settings/Notifications/DeleteView',
    'Mixins/AsModelBoundView'],
    function (App, Marionette, DeleteView, AsModelBoundView) {

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

    return AsModelBoundView.call(view);

});
