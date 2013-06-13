'use strict';
define(['app',
        'Settings/Indexers/ItemView',
        'Settings/Indexers/EditView',
        'Settings/SyncNotification'],
    function () {
    NzbDrone.Settings.Indexers.CollectionView = Backbone.Marionette.CompositeView.extend({
        itemView                : NzbDrone.Settings.Indexers.ItemView,
        itemViewContainer       : '#x-indexers',
        template                : 'Settings/Indexers/CollectionTemplate',

        events: {
            'click .x-add': 'openSchemaModal'
        },

        initialize: function () {
            NzbDrone.vent.on(NzbDrone.Commands.SaveSettings, this._saveSettings, this);
            this.savedCount = 0;
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

        _saveSettings: function () {
            var self = this;

            _.each(this.collection.models, function (model, index, list) {
                model.saveIfChanged(NzbDrone.Settings.SyncNotificaiton.callback({
                    errorMessage: 'Failed to save indexer: ' + model.get('name'),
                    successCallback: self._saveSuccessful,
                    context: self
                }));
            });

            if (self.savedCount > 0) {
                NzbDrone.Shared.Messenger.show({message: 'Indexer settings saved'});
            }

            this.savedCount = 0;
        },
        _saveSuccessful: function () {
            this.savedCount++;
        }
    });
});
