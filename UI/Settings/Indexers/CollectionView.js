'use strict';
define(['app',
    'marionette',
    'Shared/Messenger',
    'Settings/Indexers/ItemView',
    'Settings/Indexers/EditView',
    'Settings/Indexers/Collection'],
    function (App, Marionette, Messenger, IndexerItemView, IndexerEditView, IndexerCollection) {
        return Marionette.CompositeView.extend({
            itemView         : IndexerItemView,
            itemViewContainer: '#x-indexers',
            template         : 'Settings/Indexers/CollectionTemplate',

            events: {
                'click .x-add': 'openSchemaModal'
            },

            initialize: function () {
                this.listenTo(App.vent, App.Commands.SaveSettings, this._saveSettings);
                this.savedCount = 0;
            },

            openSchemaModal: function () {
                var self = this;
                //TODO: Is there a better way to deal with changing URLs?
                var schemaCollection = new IndexerCollection();
                schemaCollection.url = '/api/indexer/schema';
                schemaCollection.fetch({
                    success: function (collection) {
                        collection.url = '/api/indexer';
                        var model = _.first(collection.models);
                        model.set('id', undefined);
                        model.set('name', '');

                        var view = new IndexerEditView({ model: model, indexerCollection: self.collection});
                        App.modalRegion.show(view);
                    }
                });
            },

            _saveSettings: function () {
                var self = this;

                _.each(this.collection.models, function (model, index, list) {
                    model.saveIfChanged(NzbDrone.Settings.SyncNotificaiton.callback({
                        errorMessage   : 'Failed to save indexer: ' + model.get('name'),
                        successCallback: self._saveSuccessful,
                        context        : self
                    }));
                });

                if (self.savedCount > 0) {
                    Messenger.show({message: 'Indexer settings saved'});
                }

                this.savedCount = 0;
            },

            _saveSuccessful: function () {
                this.savedCount++;
            }
        });
    });
