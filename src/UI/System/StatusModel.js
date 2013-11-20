'use strict';
define(
    [
        'backbone',
        'api!system/status'
    ], function (Backbone, statusData) {

        var StatusModel = Backbone.Model.extend({
            url: window.NzbDrone.ApiRoot + '/system/status'
        });

        var instance = new StatusModel(statusData);
        return instance;
    });
