﻿'use strict';
define(
    [
        'backbone',
        'api!config/ui'
    ], function (Backbone, uiSettings) {
        var UiSettings = Backbone.Model.extend({

            url : window.NzbDrone.ApiRoot + '/config/ui',

            shortDateTime : function () {
                return this.get('shortDateFormat') + ' ' + this.get('timeFormat').replace('(', '').replace(')', '');
            },

            longDateTime : function () {
                return this.get('longDateFormat') + ' ' + this.get('timeFormat').replace('(', '').replace(')', '');
            }
        });

        var instance = new UiSettings(uiSettings);
        return instance;
    });
