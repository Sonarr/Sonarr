define(['app', 'Quality/QualityProfileCollection'], function (app, qualityProfileCollection) {
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

                if (episodeCount > 0)
                    percent = episodeFileCount / episodeCount * 100;

                return percent;
            }
        },

        defaults: {
            episodeFileCount: 0,
            episodeCount: 0,
            qualityProfiles: qualityProfileCollection
        }
    });

});
