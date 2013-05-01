"use strict";
define(['app', 'Quality/QualitySizeModel'], function () {
    NzbDrone.Quality.QualitySizeCollection = Backbone.Collection.extend({
        model: NzbDrone.Quality.QualitySizeModel,
        url  : NzbDrone.Constants.ApiRoot + '/qualitysizes'
    });
});
