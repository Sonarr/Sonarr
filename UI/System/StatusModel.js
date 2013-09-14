'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {

        var model = Backbone.Model.extend({
            url: window.NzbDrone.ApiRoot + '/system/status'
        });


        var instance = new model();
        instance.fetch();
        return instance;
    });
