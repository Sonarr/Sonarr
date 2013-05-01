"use strict";
define(['app', 'Quality/QualityProfileModel'], function () {

    var qualityProfileCollection = Backbone.Collection.extend({
        model: NzbDrone.Quality.QualityProfileModel,
        url  : NzbDrone.Constants.ApiRoot + '/qualityprofiles'
    });

    var profiles = new qualityProfileCollection();

    profiles.fetch();

    return profiles;
});
