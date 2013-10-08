﻿'use strict';
define(
    [
        'Quality/QualitySizeModel'
    ], function (QualitySizeModel) {
        return Backbone.Collection.extend({
            model: QualitySizeModel,
            url  : window.NzbDrone.ApiRoot + '/qualitysize'
        });
    });
