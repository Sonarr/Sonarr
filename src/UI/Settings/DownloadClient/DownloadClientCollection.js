'use strict';
define(
    [
        'backbone',
        'Settings/DownloadClient/DownloadClientModel'
    ], function (Backbone, DownloadClientModel) {

        return Backbone.Collection.extend({
            model: DownloadClientModel,
            url  : window.NzbDrone.ApiRoot + '/downloadclient'
        });
    });
