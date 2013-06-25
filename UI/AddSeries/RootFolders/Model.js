'use strict';
define(
    [
        'backbone',
        'sugar'
    ], function (Backbone) {
        return Backbone.Model.extend({
            defaults: {
                freeSpace: 0
            }
        });
    });
