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
                numberStyle        : 'S{season:00}E{episode:00}',
                multiEpisodeStyle  : 0
            }
        });
    });
