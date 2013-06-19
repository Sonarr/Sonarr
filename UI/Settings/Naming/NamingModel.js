"use strict";
define(['app',
    'Mixins/AsChangeTrackingModel'], function (App, AsChangeTrackingModel) {
    var model = Backbone.Model.extend({
        url: App.Constants.ApiRoot + '/config/naming'
    });

    return AsChangeTrackingModel.call(model);

});
