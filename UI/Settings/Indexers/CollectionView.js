'use strict';
define(['app',
        'Settings/Indexers/ItemView',
        'Settings/Indexers/EditView'],
    function () {
    NzbDrone.Settings.Indexers.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Indexers.ItemView,
        itemViewContainer       : '#x-indexers',
        template                : 'Settings/Indexers/CollectionTemplate',

        events: {
            'click .x-add': 'openSchemaModal'
        },

        initialize: function () {
            NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this.saveSettings, this);
        },

        openSchemaModal: function () {
            var self = this;
            //TODO: Is there a better way to deal with changing URLs?
            var schemaCollection = new NzbDrone.Settings.Indexers.Collection();
            schemaCollection.url = '/api/indexer/schema';
            schemaCollection.fetch({
                success: function (collection) {
                    collection.url = '/api/indexer';
                    var model = _.first(collection.models);
                    model.set('id', undefined);
                    model.set('name', '');

                    var view = new NzbDrone.Settings.Indexers.EditView({ model: model, indexerCollection: self.collection});
                    NzbDrone.modalRegion.show(view);
                }
            });
        },

        saveSettings: function () {
            //TODO: check if any models in the collection have changed and sync them only
//            this.collection.sync();
//            if (!this.model.isSaved) {
//                this.model.save(undefined, this.syncNotification("Naming Settings Saved", "Couldn't Save Naming Settings"));
//            }
        },

        syncNotification: function (success, error) {
            return {
                success: function () {
                    NzbDrone.Shared.Messenger.show({message: 'General Settings Saved'});
                },
                error  : function () {
                    NzbDrone.Shared.Messenger.show({message: "Couldn't Save General Settings", type: 'error'});
                }
            };
        }
    });
});
