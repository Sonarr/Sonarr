﻿'use strict';
define(
    [
        'backbone',
        'Mixins/AsChangeTrackingModel'
    ], function (Backbone, AsChangeTrackingModel) {
        var model = Backbone.Model.extend({

            url: window.ApiRoot + '/settings/host',

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
