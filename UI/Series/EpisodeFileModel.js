'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return Backbone.Model.extend({
            url: window.ApiRoot + '/episodefile'
        });
    });
