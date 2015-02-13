var Marionette = require('marionette');
var DownloadClientCollection = require('./DownloadClientCollection');
var DownloadClientCollectionView = require('./DownloadClientCollectionView');
var DownloadHandlingView = require('./DownloadHandling/DownloadHandlingView');
var DroneFactoryView = require('./DroneFactory/DroneFactoryView');
var RemotePathMappingCollection = require('./RemotePathMapping/RemotePathMappingCollection');
var RemotePathMappingCollectionView = require('./RemotePathMapping/RemotePathMappingCollectionView');

module.exports = Marionette.Layout.extend({
    template : 'Settings/DownloadClient/DownloadClientLayoutTemplate',

    regions : {
        downloadClients    : '#x-download-clients-region',
        downloadHandling   : '#x-download-handling-region',
        droneFactory       : '#x-dronefactory-region',
        remotePathMappings : '#x-remotepath-mapping-region'
    },

    initialize : function() {
        this.downloadClientsCollection = new DownloadClientCollection();
        this.downloadClientsCollection.fetch();
        this.remotePathMappingCollection = new RemotePathMappingCollection();
        this.remotePathMappingCollection.fetch();
    },

    onShow : function() {
        this.downloadClients.show(new DownloadClientCollectionView({ collection : this.downloadClientsCollection }));
        this.downloadHandling.show(new DownloadHandlingView({ model : this.model }));
        this.droneFactory.show(new DroneFactoryView({ model : this.model }));
        this.remotePathMappings.show(new RemotePathMappingCollectionView({ collection : this.remotePathMappingCollection }));
    }
});