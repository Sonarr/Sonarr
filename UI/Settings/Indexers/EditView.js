"use strict";

define([
    'app',
    'Settings/Indexers/Model'

], function () {

    NzbDrone.Settings.Indexers.EditView = Backbone.Marionette.ItemView.extend({
        template  : 'Settings/Indexers/EditTemplate',

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
                    NzbDrone.Shared.Messenger.show({
                        message: success
                    });

                    context.indexerCollection.add(context.model);
                    NzbDrone.modalRegion.closeModal();
                },

                error: function () {
                    window.alert(error);
                }
            };
        }
    });
});
