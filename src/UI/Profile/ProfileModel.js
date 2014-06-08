﻿'use strict';
define(
    [
        'backbone.deepmodel'
    ], function (DeepModel) {
        return DeepModel.DeepModel.extend({
            defaults: {
                id    : null,
                name  : '',
                cutoff: null
            }
        });
    });

