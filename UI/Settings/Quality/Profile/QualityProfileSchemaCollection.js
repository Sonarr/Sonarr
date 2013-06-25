"use strict";

define(
    [
        'backbone',
        'Quality/QualityProfileModel'
    ], function (Backbone, QualityProfileModel) {

        return Backbone.Collection.extend({
            model: QualityProfileModel,
            url  : window.ApiRoot + '/qualityprofiles/schema'
        });
    });
