'use strict';
define([
    'backbone',
    'Settings/DownloadClient/RemotePathMapping/RemotePathMappingModel'
], function (Backbone, RemotePathMappingModel) {

    return Backbone.Collection.extend({
        model : RemotePathMappingModel,
        url   : window.NzbDrone.ApiRoot + '/remotePathMapping'
    });
});
