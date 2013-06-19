"use strict";

define([
    'app',
    'marionette',
    'Shared/Messenger',
    'Mixins/AsModelBoundView'

], function (App, Marionette, Messenger, AsModelBoundView) {

    var view = Marionette.ItemView.extend({
        template: 'Settings/Indexers/EditTemplate',

        events: {
            'click .x-save': 'save'
        },

        initialize: function (options) {
            this.indexerCollection = options.indexerCollection;
        },

        save: function () {
            this.model.save(undefined, this.syncNotification("Indexer Saved", "Couldn't Save Indexer", this));
        },

        syncNotification: function (success, error, context) {
            return {
                success: function () {
                    Messenger.show({
                        message: success
                    });

                    context.indexerCollection.add(context.model);
                    App.modalRegion.closeModal();
                },

                error: function () {
                    window.alert(error);
                }
            };
        }
    });

    return AsModelBoundView.call(view);

});
