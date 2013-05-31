"use strict";
define(['app'], function () {
    NzbDrone.Quality.QualityProfileModel = Backbone.DeepModel.extend({

        defaults: {
            id    : null,
            name  : '',
            cutoff: null
        }
    });
});

