'use strict';
define(
    [
        'backbone',

        'underscore'
    ], function (Backbone, _) {
        return Backbone.Model.extend({

            urlRoot: ApiRoot + '/series',

            defaults: {
                episodeFileCount: 0,
                episodeCount    : 0,
                isExisting      : false,
                status          : 0
            }
        });
    });
