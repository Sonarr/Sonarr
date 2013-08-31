'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({
            urlRoot : window.ApiRoot + '/rootfolder',
            defaults: {
                freeSpace: 0
            }
        });
    });
