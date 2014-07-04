'use strict';
define(
    [
        'underscore',
        'backbone'
    ], function (_, Backbone) {
        return Backbone.Model.extend({
            defaults: {
                'target' : '/nzbdrone/route',
                'title'  : '',
                'active' : false,
                'tooltip': undefined
            }
        });
    });
