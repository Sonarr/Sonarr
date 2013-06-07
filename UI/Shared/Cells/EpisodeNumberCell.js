"use strict";
NzbDrone.Shared.Cells.EpisodeNumberCell = Backgrid.Cell.extend({

    className: "episode-number-cell",

    render: function () {

        var airDate = this.model.get(this.column.get("airDate"));

        var result = 'Unknown';

        if (airDate) {

            result = new Date(airDate).toLocaleDateString();
        }
        else {
            var season = this.model.get(this.column.get("season")).pad(2);

            var episodes = _.map(this.model.get(this.column.get("episodes")), function (episodeNumber) {
                return episodeNumber.pad(2);
            });

            result = 'S{0}-E{1}'.format(season, episodes.join());
        }

        this.$el.html(result);
        this.delegateEvents();
        return this;
    }
});
