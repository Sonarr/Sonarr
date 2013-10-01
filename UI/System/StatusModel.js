'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {

        var StatusModel = Backbone.Model.extend({
            url: window.NzbDrone.ApiRoot + '/system/status'
        });


        var instance = new StatusModel();
        instance.fetch();
        return instance;
    });
