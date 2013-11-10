'use strict';
define(
    [
        'backbone'
    ], function (Backbone) {
        return  Backbone.Model.extend({
            defaults: {
                includeSeriesTitle : true,
                includeEpisodeTitle: true,
                includeQuality     : true,
                replaceSpaces      : false,
                separator          : ' - ',
                numberStyle        : '2',
                multiEpisodeStyle  : 0
            }
        });
    });
