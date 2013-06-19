"use strict";
define(['app',
    'backbone',
    'Mixins/SaveIfChangedModel'], function (App, Backbone, AsChangeTrackingModel) {
    var model = Backbone.Model.extend({
        url: App.Constants.ApiRoot + '/settings'
    });

    return AsChangeTrackingModel.call(model);
});
