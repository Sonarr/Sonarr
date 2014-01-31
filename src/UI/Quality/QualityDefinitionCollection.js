﻿'use strict';
define(
    [
        'backbone',
        'Quality/QualityDefinitionModel'
    ], function (Backbone, QualityDefinitionModel) {
        return Backbone.Collection.extend({
            model: QualityDefinitionModel,
            url  : window.NzbDrone.ApiRoot + '/qualitydefinition'
        });
    });
