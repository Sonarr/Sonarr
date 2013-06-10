"use strict";

define(['app', 'Cells/NzbDroneCell'], function () {
    NzbDrone.Cells.EpisodeNumberCell = NzbDrone.Cells.NzbDroneCell.extend({

        className: "episode-number-cell",

        render: function () {

            this.$el.empty();

            var airDateField = this.column.get('airDate') || 'airDate';
            var seasonFiled = this.column.get('seasonNumber') || 'seasonNumber';
            var episodeFiled = this.column.get('episodes') || 'episodeNumber';

            if (this.cellValue) {

                var airDate = this.cellValue.get(airDateField);
                var seasonNumber = this.cellValue.get(seasonFiled);
                var episodes = this.cellValue.get(episodeFiled);

                var result = 'Unknown';

                if (episodes) {

                    var paddedEpisodes;

                    if (episodes.constructor === Array) {
                        paddedEpisodes = _.map(episodes,function (episodeNumber) {
                            return episodeNumber.pad(2);
                        }).join();
                    }
                    else {
                        paddedEpisodes = episodes.pad(2);
                    }

                    result = 'S{0}-E{1}'.format(seasonNumber.pad(2), paddedEpisodes);
                }
                else if (airDate) {
                    result = new Date(airDate).toLocaleDateString();
                }

                this.$el.html(result);
            }
            this.delegateEvents();
            return this;
        }
    });
});
