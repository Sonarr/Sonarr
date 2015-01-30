'use strict';

define([
    'underscore',
    'AppLayout',
    'backbone',
    'Settings/DownloadClient/DownloadClientCollection',
    'Settings/DownloadClient/Add/DownloadClientAddCollectionView'
], function (_, AppLayout, Backbone, SchemaCollection, AddCollectionView) {
    return ({

        open: function (collection) {
            var schemaCollection = new SchemaCollection();
            var originalUrl = schemaCollection.url;
            schemaCollection.url = schemaCollection.url + '/schema';
            schemaCollection.fetch();
            schemaCollection.url = originalUrl;

            var groupedSchemaCollection = new Backbone.Collection();

            schemaCollection.on('sync', function() {

               var groups = schemaCollection.groupBy(function(model, iterator) { return model.get('protocol'); });
               
               var modelCollection = _.map(groups, function(values, key, list) { 
                  return { 'header': key, collection: values };
               });
               
               groupedSchemaCollection.reset(modelCollection);
            });

            var view = new AddCollectionView({ collection: groupedSchemaCollection, targetCollection: collection });
            AppLayout.modalRegion.show(view);
        }
    });
});
