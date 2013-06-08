"use strict";
define(['app', 'Quality/QualityProfileCollection'], function (app, qualityProfiles) {
    NzbDrone.Series.SeriesModel = Backbone.Model.extend({

        urlRoot: NzbDrone.Constants.ApiRoot + '/series',

        mutators: {
            percentOfEpisodes: function () {
                var episodeCount = this.get('episodeCount');
                var episodeFileCount = this.get('episodeFileCount');

                var percent = 100;

                if (episodeCount > 0) {
                    percent = episodeFileCount / episodeCount * 100;
                }

                return percent;
            },
            banner           : function () {
                return "/mediacover/" + this.get('id') + "/banner.jpg";
            },
            poster           : function () {
                return "/mediacover/" + this.get('id') + "/poster.jpg";
            },
            fanArt           : function () {
                return "/mediacover/" + this.get('id') + "/fanart.jpg";
            },
            traktUrl         : function () {
                return "http://trakt.tv/show/" + this.get('titleSlug');
            },
            imdbUrl          : function () {
                return "http://imdb.com/title/" + this.get('imdbId');
            },
            isContinuing     : function () {
                return this.get('status') === 'continuing';
            },
            shortDate        : function () {
                var date = this.get('nextAiring');

                if (!date) {
                    return '';
                }

                return Date.create(date).short();
            },
            route            : function () {
                return '/series/details/' + this.get('titleSlug');
                //return '/series/details/' + this.get('id');
            },

            qualityProfile: function () {
                return qualityProfiles.get(this.get('qualityProfileId')).toJSON();
            }
        },

        defaults: {
            episodeFileCount: 0,
            episodeCount    : 0,
            isExisting      : false,
            status          : 0
        }
    });

});
