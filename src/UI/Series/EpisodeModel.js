'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({

            defaults: {
                seasonNumber: 0,
                status      : 0
            },

            methodUrls: {
                'update': window.NzbDrone.ApiRoot + '/episodes'
            },

            sync: function(method, model, options) {
                if (model.methodUrls && model.methodUrls[method.toLowerCase()]) {
                    options = options || {};
                    options.url = model.methodUrls[method.toLowerCase()];
                }
                Backbone.sync(method, model, options);
            }
        });
    });
