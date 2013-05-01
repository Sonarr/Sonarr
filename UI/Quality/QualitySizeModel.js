"use strict";
define(['app'], function () {
    NzbDrone.Quality.QualitySizeModel = Backbone.Model.extend({

        initialize: function () {
            this.validators = {};
        },

        mutators: {
            thirtyMinuteSize: function () {
                return this.get('maxSize') * 30;
            },
            sixtyMinuteSize : function () {
                return this.get('maxSize') * 60;
            }
        }
    });
});
