'use strict';

define(
    [
        'backbone',
        'Quality/QualityProfileModel'
    ], function (Backbone, QualityProfileModel) {

        return Backbone.Collection.extend({
            model: QualityProfileModel,
            url  : window.NzbDrone.ApiRoot + '/qualityprofiles/schema'
        });
    });
