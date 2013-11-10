'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return  Backbone.Model.extend({
            url: window.NzbDrone.ApiRoot + '/config/naming/samples'
        });
    });
