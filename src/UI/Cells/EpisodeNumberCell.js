var NzbDroneCell = require('./NzbDroneCell');
var FormatHelpers = require('../Shared/FormatHelpers');
var _ = require('underscore');

module.exports = NzbDroneCell.extend({
    className : 'episode-number-cell',

    render : function() {

        this.$el.empty();

        var airDateField = this.column.get('airDateUtc') || 'airDateUtc';
        var seasonField = this.column.get('seasonNumber') || 'seasonNumber';
        var episodeField = this.column.get('episodes') || 'episodeNumber';
        var absoluteEpisodeField = 'absoluteEpisodeNumber';

        if (this.model) {
            var result = 'Unknown';

            var airDate = this.model.get(airDateField);
            var seasonNumber = this.model.get(seasonField);
            var episodes = this.model.get(episodeField);
            var absoluteEpisodeNumber = this.model.get(absoluteEpisodeField);

            if (this.cellValue) {
                if (!seasonNumber) {
                    seasonNumber = this.cellValue.get(seasonField);
                }

                if (!episodes) {
                    episodes = this.cellValue.get(episodeField);
                }

                if (absoluteEpisodeNumber === undefined) {
                    absoluteEpisodeNumber = this.cellValue.get(absoluteEpisodeField);
                }

                if (!airDate) {
                    this.model.get(airDateField);
                }
            }

            if (episodes) {

                var paddedEpisodes;
                var paddedAbsoluteEpisode;

                if (episodes.constructor === Array) {
                    paddedEpisodes = _.map(episodes, function(episodeNumber) {
                        return FormatHelpers.pad(episodeNumber, 2);
                    }).join();
                } else {
                    paddedEpisodes = FormatHelpers.pad(episodes, 2);
                    paddedAbsoluteEpisode = FormatHelpers.pad(absoluteEpisodeNumber, 2);
                }

                result = '{0}x{1}'.format(seasonNumber, paddedEpisodes);

                if (absoluteEpisodeNumber !== undefined && paddedAbsoluteEpisode) {
                    result += ' ({0})'.format(paddedAbsoluteEpisode);
                }
            } else if (airDate) {
                result = new Date(airDate).toLocaleDateString();
            }

            this.$el.html(result);
        }
        this.delegateEvents();
        return this;
    }
});