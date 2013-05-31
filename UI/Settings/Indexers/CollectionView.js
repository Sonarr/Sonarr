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
            var self = this;
            
            _.each(this.collection.models, function (model, index, list) {
                var name = model.get('name');
                var error = 'Failed to save indexer: ' + name;

                model.saveIfChanged(self.syncNotification(undefined, error));
            });
        },

        syncNotification: function (success, error) {
            return {
                success: function () {
                    if (success) {
                        NzbDrone.Shared.Messenger.show({message: success});
                    }
                },
                error  : function () {
                    if (error) {
                        NzbDrone.Shared.Messenger.show({message: error, type: 'error'});
                    }
                }
            };
        }
    });
});
