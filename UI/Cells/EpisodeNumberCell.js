"use strict";

define(['app', 'Cells/NzbDroneCell'], function () {
    NzbDrone.Cells.EpisodeNumberCell = NzbDrone.Cells.NzbDroneCell.extend({

        className: "episode-number-cell",

        render: function () {

            var airDate = this.cellValue.get('airDate') || this.get(this.column.get("airDate"));
            var seasonNumber = this.cellValue.get('seasonNumber') || this.model.get(this.column.get("seasonNumber"));
            var episodes = this.cellValue.get('episodeNumber') || this.model.get(this.column.get("episodes"));

            var result = 'Unknown';

            if (airDate) {

                result = new Date(airDate).toLocaleDateString();
            }
            else {

                var paddedEpisodes = _.map(episodes, function (episodeNumber) {
                    return episodeNumber.pad(2);
                });

                result = 'S{0}-E{1}'.format(seasonNumber, paddedEpisodes.join());
            }

            this.$el.html(result);
            this.delegateEvents();
            return this;
        }
    });
});
