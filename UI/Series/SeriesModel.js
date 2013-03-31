define(['app', 'Quality/QualityProfileCollection', 'AddSeries/RootFolders/RootFolderCollection'], function (app, qualityProfileCollection, rootFolders) {
    NzbDrone.Series.SeriesModel = Backbone.Model.extend({

        urlRoot: NzbDrone.Constants.ApiRoot + '/series',

        mutators: {
            bestDateString: function () {
                return bestDateString(this.get('nextAiring'));
            },

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
                var banner = _.find(this.get('images'), function (image) {
                    return image.coverType === 1;
                });

                if (banner) {
                    return banner.url;
                }

                return undefined;

            },
            traktUrl         : function () {
                return "http://trakt.tv/show/" + this.get('titleSlug');
            }
        },

        defaults: {
            episodeFileCount: 0,
            episodeCount    : 0,
            qualityProfiles : qualityProfileCollection,
            rootFolders     : rootFolders
        }
    });

});
