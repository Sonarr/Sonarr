'use strict';
define(
    [
        'backbone',
        'Update/UpdateModel'
    ], function (Backbone, UpdateModel) {
        return  Backbone.Collection.extend({
            url  : window.NzbDrone.ApiRoot + '/update',
            model: UpdateModel
        });
    });
