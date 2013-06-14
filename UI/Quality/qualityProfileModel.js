"use strict";
define(['app', 'backbone.deepmodel'], function (App, DeepModel) {
    NzbDrone.Quality.QualityProfileModel = DeepModel.DeepModel.extend({

        defaults: {
            id    : null,
            name  : '',
            cutoff: null
        }
    });
});

