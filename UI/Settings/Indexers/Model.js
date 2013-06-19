"use strict";
define([
    'backbone.deepmodel', 'Mixins/AsChangeTrackingModel'], function (DeepModel, AsChangeTrackingModel) {
    var model = DeepModel.DeepModel.extend({

    });

    return AsChangeTrackingModel.call(model);
});
