﻿'use strict';
define(
    [
        'backbone',
        'Quality/QualitySizeModel'
    ], function (Backbone, QualitySizeModel) {
        return Backbone.Collection.extend({
            model: QualitySizeModel,
            url  : window.NzbDrone.ApiRoot + '/qualitysize'
        });
    });
