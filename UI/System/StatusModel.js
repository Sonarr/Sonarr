'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {

        var model = Backbone.Model.extend({
            url: window.ApiRoot + '/system/status'
        });


        var instance = new model();
        instance.fetch();
        return instance;
    });
