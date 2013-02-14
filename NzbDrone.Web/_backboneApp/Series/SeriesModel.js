define(['app'], function () {


    NzbDrone.Series.SeriesModel = Backbone.Model.extend({
        url: NzbDrone.Constants.ApiRoot + '/series',

        mutators: {
            bestDateString: function () {
                var dateSource = this.get('nextAiring');

                if (!dateSource) return '';

                var date = Date.create(dateSource);

                if (date.isYesterday()) return 'Yesterday';
                if (date.isToday()) return 'Today';
                if (date.isTomorrow()) return 'Tomorrow';
                if (date.isToday()) return 'Today';
                if (date.isBefore(Date.create().addDays(7))) return date.format('{Weekday}');

                return date.format('{MM}/{dd}/{yyyy}');
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
            episodeCount: 0
        }
    });

});
