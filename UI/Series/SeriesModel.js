'use strict';
define(
    [
        'backbone',
        'Quality/QualityProfileCollection'
    ], function (Backbone, QualityProfileCollection) {
        return Backbone.Model.extend({

            urlRoot: ApiRoot + '/series',

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
                poster           : function () {
                    var poster = _.find(this.get('images'), function (image) {
                        return image.coverType === 'poster';
                    });

                    if (poster) {
                        return poster.url;
                    }

                    return undefined;
                },
                fanArt           : function () {
                    var poster = _.find(this.get('images'), function (image) {
                        return image.coverType === 'fanart';
                    });

                    if (poster) {
                        return poster.url;
                    }

                    return undefined;
                },
                traktUrl         : function () {
                    return 'http://trakt.tv/show/' + this.get('titleSlug');
                },
                imdbUrl          : function () {
                    return 'http://imdb.com/title/' + this.get('imdbId');
                },
                isContinuing     : function () {
                    return this.get('status') === 'continuing';
                },
                route            : function () {
                    return '/series/' + this.get('titleSlug');
                    //return '/series/details/' + this.get('id');
                },

                qualityProfile: function () {
                    var profile = QualityProfileCollection.get(this.get('qualityProfileId'));
                    if (profile) {
                        return profile.toJSON();
                    }

                    return undefined;
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
