"use strict";
define(
    [
        'backbone',
        'constants'
    ], function (Backbone) {

        var model = Backbone.Model.extend({
            url: Constants.ApiRoot + '/system/status'
        });


        var instance = new model();
        instance.fetch();
        return instance;
    });
