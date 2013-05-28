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

        openSchemaModal: function () {
            //TODO: Is there a better way to deal with changing URLs?
            var schema = new NzbDrone.Settings.Indexers.Collection();
            schema.url = '/api/indexer/schema';
            schema.fetch({
                success: function (collection) {
                    collection.url = '/api/indexer';
                    var model = _.first(collection.models);
                    model.set('id', undefined);
                    model.set('name', '');

                    var view = new NzbDrone.Settings.Indexers.EditView({ model: model});
                    NzbDrone.modalRegion.show(view);
                }
            });
        }
    });
});
