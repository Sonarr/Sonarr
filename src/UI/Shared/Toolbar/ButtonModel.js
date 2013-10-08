'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({
            defaults: {
                'target' : '/nzbdrone/route',
                'title'  : '',
                'active' : false,
                'tooltip': undefined        }
        });
    });
