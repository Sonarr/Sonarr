'use strict';
define(['app', 'backbone', 'Mixins/AsChangeTrackingModel'], function (App, Backbone, AsChangeTrackingModel) {
    var model = Backbone.Model.extend({

        url: App.Constants.ApiRoot + '/settings/host',

        initialize: function () {
            this.on('change', function () {
                this.isSaved = false;
            }, this);

            this.on('sync', function () {
                this.isSaved = true;
            }, this);
        }
    });

    return AsChangeTrackingModel.call(model);
});
